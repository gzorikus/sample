using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using YourCompany.Configuration.EFCore.PessimisticLocking;

namespace YourCompany.Configuration.EFCore.Sqlite
{
    internal sealed class SqlitePessimisticLockingUpdateInterceptor
        : ProviderSpecificPessimisticLockingUpdateInterceptor.ForSingleEntityPerCommand.ForBigintOnly
    {
        internal static SqlitePessimisticLockingUpdateInterceptor Default = new();
        private SqlitePessimisticLockingUpdateInterceptor() { }

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
            string lastModifiedAtColumnName = context.SetLastModifiedAtColumnName ?? throw new ApplicationException("context.SetLastModifiedAtColumnName == null");
            var idList = new StringBuilder();
            idList.AppendJoin(",", context.Ids);

            // SQLite uses double quotes for identifiers
            // Schema is typically ignored in SQLite
            // SQLite doesn't have OUTPUT/RETURNING, so we do UPDATE then SELECT
            // Also, SQLite doesn't support table hints like UPDLOCK
            return $@"
                -- First, update the records
                UPDATE ""{tableName}""
                SET {lastModifiedAtColumnName} = '{context.SetLastModifiedAt:yyyy-MM-dd HH:mm:ss.fff}'
                WHERE {primaryKeyProperty.Name} IN ({idList});
                
                -- Then select them with the updated values
                {originalSelectCommand}";
        }
    }
}