using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using YourCompany.Configuration.EFCore.PessimisticLocking;

namespace YourCompany.Configuration.EFCore.PostgreSQL
{
    internal sealed class NpgsqlPessimisticLockingUpdateInterceptor
        : ProviderSpecificPessimisticLockingUpdateInterceptor.ForSingleEntityPerCommand.ForBigintOnly
    {
        internal static NpgsqlPessimisticLockingUpdateInterceptor Default = new();
        private NpgsqlPessimisticLockingUpdateInterceptor() { }

        protected override string TransformToLockingCommand(
            string originalSelectCommand,
            IReadOnlyModel model,
            IReadOnlyEntityType entityType,
            IReadOnlyProperty primaryKeyProperty,
            PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<long> context)
        {
            if (originalSelectCommand == null) throw new ArgumentNullException(nameof(originalSelectCommand));
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (primaryKeyProperty == null) throw new ArgumentNullException(nameof(primaryKeyProperty));
            if (context == null) throw new ArgumentNullException(nameof(context));

            string tableName = entityType.GetTableName() ?? throw new ApplicationException("tableName == null");
            string formattedTableName = FormatTableName(tableName, entityType.GetSchema());
            string lastModifiedAtColumnName = context.SetLastModifiedAtColumnName ?? throw new ApplicationException("context.SetLastModifiedAtColumnName == null");
            var idList = new StringBuilder();
            idList.AppendJoin(",", context.Ids);

            // Use CTE to ensure atomic operation within existing transaction
            return $@"
                WITH updated AS (
                    UPDATE ""{formattedTableName}""
                    SET ""{lastModifiedAtColumnName}"" = '{context.SetLastModifiedAt:yyyy-MM-dd HH:mm:ss.fff}'
                    WHERE ""{primaryKeyProperty.Name}"" = ANY(ARRAY[{idList}]);
                    RETURNING *
                )
                {ReplaceWithCteAlias(originalSelectCommand, tableName, formattedTableName)}";
        }

        private static string FormatTableName(string tableName, string schema)
        {
            // PostgreSQL uses double quotes ""
            return string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
        }

        private static string ReplaceWithCteAlias(string originalSelectCommand, string entityTableName, string formattedTableName)
        {
            string replaced = originalSelectCommand.Replace($"FROM {formattedTableName}", "FROM updated");
            if (replaced == originalSelectCommand) replaced = originalSelectCommand.Replace($"FROM \"{entityTableName}\"", "FROM updated");
            if (replaced == originalSelectCommand) throw new ApplicationException("replaced == originalSelectCommand");
            return replaced;
        }
    }
}