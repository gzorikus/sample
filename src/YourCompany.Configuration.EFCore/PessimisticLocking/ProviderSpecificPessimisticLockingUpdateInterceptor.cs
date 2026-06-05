using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.PessimisticLocking
{
    internal abstract class ProviderSpecificPessimisticLockingUpdateInterceptor : DbCommandInterceptor
    {
        private static readonly AsyncLocal<PessimisticLockingUpdateInterceptionContext> CurrentContext = new();

        private ProviderSpecificPessimisticLockingUpdateInterceptor() { }

        internal static void StartAsyncLocalInterception(PessimisticLockingUpdateInterceptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (CurrentContext.Value != null) throw new ApplicationException("CurrentContext.Value != null");
            CurrentContext.Value = context;
        }

        internal static void StopAsyncLocalInterception(PessimisticLockingUpdateInterceptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (CurrentContext.Value != context) throw new ApplicationException("CurrentContext.Value != context");
            CurrentContext.Value = null;
        }

        public sealed override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            var context = CurrentContext.Value;
            if (context != null && command.CommandText.Contains(context.EFPrefixedTag))
                throw new NotSupportedException("use with async only");
            return result;
        }

        public sealed override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            var context = CurrentContext.Value;
            if (context == null || !command.CommandText.Contains(context.EFPrefixedTag))
                return new ValueTask<InterceptionResult<DbDataReader>>(result);

            if (eventData.Context.Database.CurrentTransaction == null) throw new ApplicationException("eventData.Context.Database.CurrentTransaction == null");

            string transformed = TransformToLockingCommand(
                command.CommandText, eventData.Context.Model, CurrentContext.Value)
                    ?? throw new ApplicationException("transformed == null");
            if (transformed == command.CommandText) throw new ApplicationException("transformed == command.CommandText");
            command.CommandText = transformed;
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }

        protected abstract string TransformToLockingCommand(
            string originalSelectCommand, IReadOnlyModel model, PessimisticLockingUpdateInterceptionContext context);

        internal abstract class ForSingleEntityPerCommand : ProviderSpecificPessimisticLockingUpdateInterceptor
        {
            protected sealed override string TransformToLockingCommand(
                string originalSelectCommand, IReadOnlyModel model, PessimisticLockingUpdateInterceptionContext context)
            {
                if (originalSelectCommand == null) throw new ArgumentNullException(nameof(originalSelectCommand));
                if (model == null) throw new ArgumentNullException(nameof(model));
                if (context == null) throw new ArgumentNullException(nameof(context));
                if (context is not PessimisticLockingUpdateInterceptionContext.ForSingleDbObject forSingleDbObject)
                    throw new ApplicationException("context is not PessimisticLockingUpdateInterceptionContext.ForSingleDbObject forSingleDbObject");

                var entityTypeInfo = forSingleDbObject.EntityName != null
                    ? model.FindEntityType(forSingleDbObject.EntityName)
                    : model.FindEntityType(forSingleDbObject.EntityType);
                if (string.IsNullOrEmpty(entityTypeInfo?.GetTableName())) throw new ApplicationException("string.IsNullOrEmpty(entityTypeInfo?.GetTableName())");
                var primaryKey = entityTypeInfo.FindPrimaryKey() ?? throw new ApplicationException("entityTypeInfo.FindPrimaryKey() == null");
                return TransformToLockingCommand(originalSelectCommand, model, entityTypeInfo, primaryKey, forSingleDbObject);
            }

            protected abstract string TransformToLockingCommand(string originalSelectCommand,
                IReadOnlyModel model,
                IReadOnlyEntityType entityType,
                IReadOnlyKey primaryKey,
                PessimisticLockingUpdateInterceptionContext.ForSingleDbObject context);

            internal abstract class ForBigintOnly : ForSingleIdTypeOnly<long> { }
            internal abstract class ForSingleIdTypeOnly<TId> : ForSingleEntityPerCommand
            {
                protected sealed override string TransformToLockingCommand(string originalSelectCommand,
                    IReadOnlyModel model,
                    IReadOnlyEntityType entityType,
                    IReadOnlyKey primaryKey,
                    PessimisticLockingUpdateInterceptionContext.ForSingleDbObject context)
                {
                    if (originalSelectCommand == null) throw new ArgumentNullException(nameof(originalSelectCommand));
                    if (model == null) throw new ArgumentNullException(nameof(model));
                    if (entityType == null) throw new ArgumentNullException(nameof(entityType));
                    if (primaryKey == null) throw new ArgumentNullException(nameof(primaryKey));
                    if (primaryKey.Properties.Count != 1) throw new ApplicationException("primaryKey.Properties.Count != 1");
                    if (context == null) throw new ArgumentNullException(nameof(context));
                    if (context is not PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<TId> withIds) throw new ApplicationException("context is not PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<TId> withIds");
                    var primaryKeyProperty = primaryKey.Properties[0] ?? throw new ApplicationException("primaryKey.Properties[0] == null");
                    if (primaryKeyProperty.ClrType != typeof(TId)) throw new ApplicationException("primaryKeyProperty.ClrType != typeof(TId)");
                    return TransformToLockingCommand(originalSelectCommand, model, entityType, primaryKeyProperty, withIds);
                }

                protected abstract string TransformToLockingCommand(string originalSelectCommand,
                    IReadOnlyModel model,
                    IReadOnlyEntityType entityType,
                    IReadOnlyProperty primaryKeyProperty,
                    PessimisticLockingUpdateInterceptionContext.ForSingleDbObject.WithIds<TId> context);
            }
        }
    }
}