using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper.PostgreSQL
{
    internal static class NpgsqlAccountSqlHelper
    {
        internal static string BuildJsonbChangesArraySourceSql(
            IReadOnlyList<DapperAccountState> states, NpgsqlParameterCollection parameters)
        {
            if (states == null) throw new ArgumentNullException(nameof(states));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var batchChanges = new NpgsqlDynamicJsonAccountChanges[states.Count];
            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i] ?? throw new ApplicationException("states[i] == null");

                var recordChanges = new NpgsqlDynamicJsonAccountChanges
                {
                    ResolvingFromId = state.Identity.HasValue ? state.Identity.Value : null,
                    BalanceSet = state.GetSettingPropertiesBeforeDataChanging().Balance,
                    ResolvingFromOrInsertingPublicId = !state.Identity.HasValue && state.Identity.HasPublicId
                        ? state.Identity.PublicId
                        : null
                };

                var changingSpecifications = state.MatchingAfterDataChanging ?? throw new ApplicationException("state.MatchingAfterDataChanging == null");
                for (int j = 0; j < changingSpecifications.Count; j++)
                    SetRecordChangeActionProperties(ref recordChanges, changingSpecifications[j]);

                batchChanges[i] = recordChanges;
            }

            parameters.Add(new NpgsqlParameter("Jsonb", NpgsqlDbType.Jsonb) { Value = batchChanges });
            return "SELECT @Jsonb AS jsonb";
        }

        internal static string BuildJsonbQuerySourceSql(
            IReadOnlyList<ISpecification<IAccountRecordData>> batchActions,
            IReadOnlyList<ISpecification<IAccountRecordData>> batchConditions,
            bool useForUpdateSkipLocked,
            int batchSize,
            NpgsqlParameterCollection parameters)
        {
            if (batchActions == null) throw new ArgumentNullException(nameof(batchActions));
            if (batchConditions == null) throw new ArgumentNullException(nameof(batchConditions));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            string columns = "id AS resolving_from_id";

            if (batchActions != null)
                for (int i = 0; i < batchActions.Count; i++)
                    columns = AddBatchQueryActionColumns(columns, batchActions[i], parameters);

            string wherePredicates = null;

            if (batchConditions != null)
                for (int i = 0; i < batchConditions.Count; i++)
                    wherePredicates = AddBatchQueryConditionPredicates(wherePredicates, batchConditions[i], parameters);

            parameters.Add(new NpgsqlParameter<int>("BatchSize", batchSize));
            return $"""
                SELECT jsonb_agg(to_jsonb(r)) AS jsonb
                FROM (
                    SELECT {columns}
                    FROM account
                    {(wherePredicates != null ? "WHERE " : "-- NO WHERE")}{wherePredicates}
                    {(useForUpdateSkipLocked ? "FOR UPDATE SKIP LOCKED" : "-- NEVER LOCKING WITH ARBITRARY PREDICATES")}
                    LIMIT @BatchSize
                ) r
            """;
        }

        private static void SetRecordChangeActionProperties(
            ref NpgsqlDynamicJsonAccountChanges changes, ISpecification<IAccountRecordData> spec)
        {
            if (spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual)
            {
                if (changes.BalanceSet.HasValue) throw new ApplicationException("changes.BalanceSet.HasValue");
                if (changes.BalanceIncrement != null) throw new ApplicationException("changes.BalanceIncrement.HasValue");
                if (CheckToIncrementChangingBalance(greaterOrEqual.ChangeMode))
                {
                    changes.BalanceIncrement = greaterOrEqual.Threshold;
                }
                else
                {
                    changes.BalanceSet = greaterOrEqual.Threshold;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(spec), spec, message: null);
            }
        }

        private static string AddBatchQueryActionColumns(
            string columns, ISpecification<IAccountRecordData> spec, NpgsqlParameterCollection parameters)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual)
            {
                if (CheckToIncrementChangingBalance(greaterOrEqual.ChangeMode))
                {
                    parameters.Add(new NpgsqlParameter<decimal>("BalanceIncrement", greaterOrEqual.Threshold));
                    columns += ", @BalanceIncrement AS balance_increment";
                }
                else
                {
                    parameters.Add(new NpgsqlParameter<decimal>("BalanceSet", greaterOrEqual.Threshold));
                    columns += ", @BalanceSet AS balance_set";
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(spec), spec, message: null);
            }

            return columns;
        }

        private static bool CheckToIncrementChangingBalance(
            AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode mode)
        {
            switch (mode)
            {
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Increment:
                    return true;
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Set:
                    return false;
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.NoChange:
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, message: null);
            }
        }

        private static string AddBatchQueryConditionPredicates(
            string wherePredicates, ISpecification<IAccountRecordData> spec, NpgsqlParameterCollection parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            string predicate;

            if (spec is AccountSpecifications.Triggering.BalanceThreshold.LessThan less)
            {
                parameters.Add(new NpgsqlParameter<decimal>("BalanceLessThreshold", less.Threshold));
                predicate = "balance < @BalanceLessThreshold";
            }
            else if (spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual)
            {
                if (greaterOrEqual.ChangeMode
                    != AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.NoChange)
                    throw new ApplicationException("greaterOrEqual.ChangeMode != NoChange");
                parameters.Add(new NpgsqlParameter<decimal>("BalanceGreaterOrEqualThreshold", greaterOrEqual.Threshold));
                predicate = "balance >= @BalanceGreaterOrEqualThreshold";
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(spec), spec, message: null);
            }

            return wherePredicates == null ? predicate : $"""
                {wherePredicates}
                AND {predicate}
            """;
        }

        internal static string BuildFullCte(string jsonbSourceSql)
        {
            if (jsonbSourceSql == null) throw new ArgumentNullException(nameof(jsonbSourceSql));

            return $"""
                WITH jsonb_source AS ({jsonbSourceSql})
                , records_with_changes AS (
                    SELECT COALESCE(j.resolving_from_id, pid.account_id) AS resolving_from_id
                    , j.balance_set
                    , j.balance_increment
                    , j.resolving_from_or_inserting_public_id AS resolved_from_or_inserting_public_id
                    FROM jsonb_source
                    CROSS JOIN LATERAL JSONB_TO_RECORDSET(jsonb_source.jsonb) AS j (
                        resolving_from_id bigint,
                        balance_set numeric,
                        balance_increment numeric,
                        resolving_from_or_inserting_public_id uuid)
                    LEFT JOIN account_public_id pid ON pid.public_id = j.resolving_from_or_inserting_public_id
                )
                , lock_records_to_update_wo_deadlocks AS (
                    SELECT upd.*
                    , locked.balance AS old_balance -- for < v18
                    FROM records_with_changes upd
                    JOIN account locked ON locked.id = upd.resolving_from_id
                    ORDER BY locked.id
                    FOR UPDATE OF locked
                )
                , updated_records AS (
                    UPDATE account AS locked
                    SET balance = CASE
                        WHEN upd.balance_set IS NOT NULL THEN upd.balance_set
                        WHEN upd.balance_increment IS NOT NULL THEN locked.balance + upd.balance_increment
                        ELSE locked.balance
                    END
                    , last_modified_at = NOW()
                    FROM lock_records_to_update_wo_deadlocks upd
                    JOIN account_public_id pid ON upd.resolving_from_id = pid.account_id
                    WHERE locked.id = upd.resolving_from_id
                    RETURNING upd.resolving_from_id AS existing_record_id
                    , upd.old_balance -- for < v18
                    -- , OLD.balance AS old_balance -- for >= v18, but not much sence since can't garant locking order w/o FOR UPDATE see lock_records_to_update_wo_deadlocks instead
                    , pid.public_id AS existing_public_id
                )
                , inserted_public_ids AS (
                    INSERT INTO account_public_id (public_id)
                    SELECT ins.resolved_from_or_inserting_public_id
                    FROM records_with_changes ins
                    WHERE ins.resolving_from_id IS NULL
                    RETURNING account_id, public_id
                )
                , inserted_records AS (
                    INSERT INTO account (
                        id,
                        balance,
                        last_modified_at)
                    SELECT ipi.account_id
                    , CASE
                        WHEN ins.balance_set IS NOT NULL THEN ins.balance_set
                        WHEN ins.balance_increment IS NOT NULL THEN ins.balance_increment
                        ELSE {Account.BalanceDefault}
                    END
                    , NOW()
                    FROM records_with_changes ins
                    JOIN inserted_public_ids ipi ON ipi.public_id = ins.resolved_from_or_inserting_public_id
                    RETURNING id AS inserted_record_id
                )
                SELECT COALESCE(upd.existing_record_id, ins.inserted_record_id) AS "ResolvedId"
                , COALESCE(upd.old_balance, {Account.BalanceDefault}) AS "Balance"
                , COALESCE(upd.existing_public_id, ipi.public_id) AS "ResolvedPublicId"
                FROM records_with_changes ups
                LEFT JOIN updated_records upd ON upd.existing_record_id = ups.resolving_from_id
                LEFT JOIN inserted_public_ids ipi ON ipi.public_id = ups.resolved_from_or_inserting_public_id
                LEFT JOIN inserted_records ins ON ins.inserted_record_id = ipi.account_id
            """;
        }
    }
}