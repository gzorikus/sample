using System;
using System.Collections.Generic;
using YourCompany.Configuration.EFCore.PessimisticLocking;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        public PessimisticLockingUpdateInterceptionContext StartPessimisticLockingInterception<TId>(
            Type entityType,
            IReadOnlyList<TId> ids,
            string lastModifiedAtColumnName = PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName)
        {
            var context = new PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<TId>(entityType, ids)
            {
                SetLastModifiedAt = ConfiguredUtcNow,
                SetLastModifiedAtColumnName = lastModifiedAtColumnName
                    ?? PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName
            };
            ProviderSpecificPessimisticLockingUpdateInterceptor.StartAsyncLocalInterception(context);
            return context;
        }

        public PessimisticLockingUpdateInterceptionContext StartPessimisticLockingInterception<TId>(
            string entityName,
            IReadOnlyList<TId> ids,
            string lastModifiedAtColumnName = PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName)
        {
            if (entityName == null) throw new ArgumentNullException(nameof(entityName));

            var entityType = SingleRuntimeModelPerConfigurationType.FindEntityType(entityName)?.ClrType
                ?? throw new ApplicationException("SingleRuntimeModelPerConfigurationType.FindEntityType(entityName)?.ClrType == null");

            var context = new PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<TId>(entityType, ids)
            {
                EntityName = entityName,
                SetLastModifiedAt = ConfiguredUtcNow,
                SetLastModifiedAtColumnName = lastModifiedAtColumnName
                    ?? PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName
            };
            ProviderSpecificPessimisticLockingUpdateInterceptor.StartAsyncLocalInterception(context);
            return context;
        }
    }
}