using System;
using System.Collections.Generic;

namespace YourCompany.OLTP.StateOwnership
{
    public interface IState<TRecordData> where TRecordData : class
    {
        Identity Identity { get; }
        IStateModifying<TRecordData> Modifying { get; }
        TRecordData DataAfterAccess { get; }
    }

    public static class StateExtensions
    {
        public static bool CheckIsTriggeringSpecificationToMatchWithoutDataChanges<TRecordData>(
            this IState<TRecordData> state, EventArgs transactionCallbackArgs)
            where TRecordData : class
        {
            state.EnsureIsModifyingTransaction();
            var specification = transactionCallbackArgs as ISpecification<TRecordData>;
            if (specification == null) return false;
            state.EnsureHasSingleSpecificationToMatchWithoutDataChanges(specification);
            return true;
        }

        public static void EnsureHasSingleSpecificationToMatchWithoutDataChanges<TRecordData>(
            this IState<TRecordData> state, ISpecification<TRecordData> specification)
            where TRecordData : class
        {
            if (!state.CheckHasSingleSpecificationToMatchWithoutDataChanges(specification))
                throw new ApplicationException("!state.CheckHasSingleSpecificationToMatchWithoutDataChanges(specification)");
        }

        public static bool CheckHasSingleSpecificationToMatchWithoutDataChanges<TRecordData>(
            this IState<TRecordData> state, ISpecification<TRecordData> specification)
            where TRecordData : class
        {
            var matching = GetSpecificationsToMatchWithoutDataChanges(state);
            return CheckHasSingleMatching(matching, specification);
        }

        public static IReadOnlyList<ISpecification<TRecordData>> GetSpecificationsToMatchWithoutDataChanges<TRecordData>(
            IState<TRecordData> state)
            where TRecordData : class
            => state.ForModifying().MatchingWithoutDataChanges ?? throw new ApplicationException("state.ForModifying().MatchingWithoutDataChanges == null");

        public static bool CheckIsTriggeringSpecificationForDataChanging<TRecordData>(
            this IState<TRecordData> state, EventArgs transactionCallbackArgs)
            where TRecordData : class
        {
            state.EnsureIsModifyingTransaction();
            var specification = transactionCallbackArgs as ISpecification<TRecordData>;
            if (specification == null) return false;
            EnsureHasSingleSpecificationForDataChanging(state, specification);
            return true;
        }

        public static void EnsureHasSingleSpecificationForDataChanging<TRecordData>(
            this IState<TRecordData> state, ISpecification<TRecordData> specification)
            where TRecordData : class
        {
            if (!state.CheckHasSingleSpecificationForDataChanging(specification))
                throw new ApplicationException("!state.CheckHasSingleSpecificationForDataChanging(specification)");
        }

        public static bool CheckHasSingleSpecificationForDataChanging<TRecordData>(
            this IState<TRecordData> state, ISpecification<TRecordData> specification)
            where TRecordData : class
        {
            var matching = GetSpecificationsForDataChanging(state);
            return CheckHasSingleMatching(matching, specification);
        }

        public static IReadOnlyList<ISpecification<TRecordData>> GetSpecificationsForDataChanging<TRecordData>(
            IState<TRecordData> state)
            where TRecordData : class
            => state.ForModifying().MatchingAfterDataChanging ?? throw new ApplicationException("state.ForModifying().MatchingAfterDataChanging == null");

        public static TRecordData GetSettingPropertiesBeforeDataChanging<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            state.EnsureIsBeforeDataChanging();
            return GetSettingPropertiesWhileModifying(state);
        }

        public static TRecordData GetSettingPropertiesWhileRecordDataIsLocked<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            state.EnsureRecordDataIsLocked();
            return GetSettingPropertiesWhileModifying(state);
        }

        public static TRecordData GetSettingPropertiesWhileModifying<TRecordData>(IState<TRecordData> state)
            where TRecordData : class
            => state.ForModifying().SettingDataProperties ?? throw new ApplicationException("state.ForModifying().SettingDataProperties == null");

        public static TRecordData GetLockedRecordData<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            state.EnsureRecordDataIsLocked();
            return state.ForModifying().LockedRecordData ?? throw new ApplicationException("state.ForModifying().LockedRecordData == null");
        }

        public static TRecordData GetFinishedAccessData<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            state.EnsureDataAccessIsFinished();
            return state.DataAfterAccess ?? throw new ApplicationException("state.DataAfterAccess == null");
        }

        public static IStateModifying<TRecordData> ForModifying<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            state.EnsureIsModifyingTransaction();
            return state.Modifying ?? throw new ApplicationException("state.Modifying == null");
        }

        public static void EnsureIsModifyingTransaction<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException(nameof(EnsureIsModifyingTransaction));
        }

        public static void EnsureIsBeforeDataChanging<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException("!state.CheckIsModifyingTransaction()");
            if (state.CheckRecordDataIsLocked()) throw new ApplicationException("state.CheckRecordDataIsLocked()");
            if (!state.CheckIsBeforeDataChanging()) throw new ApplicationException(nameof(EnsureIsBeforeDataChanging));
        }

        public static void EnsureRecordDataIsLocked<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException("!state.CheckIsModifyingTransaction()");
            if (state.CheckDataAccessIsFinished()) throw new ApplicationException("state.CheckDataAccessIsFinished()");
            if (!state.CheckRecordDataIsLocked()) throw new ApplicationException(nameof(EnsureRecordDataIsLocked));
        }

        public static void EnsureDataAccessIsFinished<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (state.CheckIsModifyingTransaction() && state.CheckRecordDataIsLocked())
                throw new ApplicationException("state.CheckIsModifyingTransaction() && state.CheckRecordDataIsLocked()");
            if (!state.CheckDataAccessIsFinished()) throw new ApplicationException(nameof(EnsureDataAccessIsFinished));
        }

        public static bool CheckIsModifyingTransaction<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (state.Modifying == null) return false;
            if (state.Modifying.MatchingWithoutDataChanges == null) throw new ApplicationException("state.Modifying.MatchingWithoutDataChanges == null");
            if (state.Modifying.MatchingAfterDataChanging == null) throw new ApplicationException("state.Modifying.MatchingAfterDataChanging == null");
            if (state.Modifying.SettingDataProperties == null) throw new ApplicationException("state.Modifying.SettingDataProperties == null");
            return true;
        }

        public static bool CheckIsBeforeDataChanging<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.CheckIsModifyingTransaction() && !state.CheckRecordDataIsLocked() && !state.CheckDataAccessIsFinished();

        public static bool CheckRecordDataIsLocked<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.CheckIsModifyingTransaction() && state.Modifying.LockedRecordData != null;

        public static bool CheckDataAccessIsFinished<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.DataAfterAccess != null;

        private static bool CheckHasSingleMatching<TRecordData>(
            IReadOnlyList<ISpecification<TRecordData>> matching, ISpecification<TRecordData> specification)
            where TRecordData : class
        {
            if (matching == null) throw new ArgumentNullException(nameof(matching));
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            ISpecification<TRecordData> singleSpecification = null;

            for (int i = 0; i < matching.Count; i++)
            {
                if (specification.Equals(matching[i]))
                {
                    if (singleSpecification != null) throw new ApplicationException("singleSpecification != null");
                    singleSpecification = specification;
                }
            }

            return singleSpecification != null;
        }
    }
}