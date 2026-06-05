using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YourCompany.Configuration.EFCore.PessimisticLocking
{
    internal abstract class PessimisticLockingUpdateInterceptionContext
    {
        internal const string DefaultLastModifiedAtColumnName = "LastModifiedAt";

        private bool _disposed;

        private PessimisticLockingUpdateInterceptionContext() { }

        internal string QueryableTagWith { get; init; } = nameof(PessimisticLockingUpdateInterceptionContext);
        internal string EFPrefixedTag => "-- " + QueryableTagWith;
        internal DateTime SetLastModifiedAt { get; init; } = DateTime.UtcNow;
        internal string SetLastModifiedAtColumnName { get; init; } = DefaultLastModifiedAtColumnName;

        internal IQueryable<T> Intercept<T>(IQueryable<T> queryable)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PessimisticLockingUpdateInterceptionContext));
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (this is not ForSingleDbObject forSingleDbObject) throw new ApplicationException("this is not ForSingleDbObject forSingleDbObject");
            if (typeof(T) != forSingleDbObject.EntityType) throw new ApplicationException("typeof(T) != forSingleDbObject.EntityType");
            return queryable.TagWith(QueryableTagWith);
        }

        internal void Dispose()
        {
            bool disposed = _disposed;
            _disposed = true;
            if (!disposed) ProviderSpecificPessimisticLockingUpdateInterceptor.StopAsyncLocalInterception(this);
        }

        internal abstract class ForSingleDbObject : PessimisticLockingUpdateInterceptionContext
        {
            internal Type EntityType { get; }
            internal string EntityName { get; init; }

            private ForSingleDbObject(Type entityType)
            {
                EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            }

            internal sealed class WithIds<TId> : ForSingleDbObject
            {
                internal IReadOnlyList<TId> Ids { get; }

                internal WithIds(Type entityType, IReadOnlyList<TId> ids)
                    : base(entityType)
                {
                    Ids = ids ?? throw new ArgumentNullException(nameof(ids));
                    if (ids.Count < 1) throw new ApplicationException("ids.Count < 1");
                }
            }
        }
    }
}