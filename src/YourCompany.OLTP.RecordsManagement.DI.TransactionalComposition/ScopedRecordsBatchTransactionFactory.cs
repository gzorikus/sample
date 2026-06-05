using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal class ScopedRecordsBatchTransactionFactory : DI.ScopedRecordsBatchTransactionFactory
    {
        private static readonly AsyncLocal<Type> CurrentlyHandledEntityRecordType = new();

        public ScopedRecordsBatchTransactionFactory(IServiceProvider provider) : base(provider) { }

        internal new ComposableRecordTypeInfo GetRecordTypeInfo(Type recordType)
        {
            if (recordType == null) throw new ArgumentNullException(nameof(recordType));
            var recordTypesMap = Provider.GetRequiredService<RecordTypesCompositionMap>();
            if (!recordTypesMap.TryGetValue(recordType, out ComposableRecordTypeInfo recordTypeInfo))
                throw new ApplicationException("!recordTypesMap.TryGetValue(recordType, out ComposableRecordTypeInfo recordTypeInfo)");
            if (recordTypeInfo.Type != recordType) throw new ApplicationException("recordTypeInfo.Type != recordType");
            return recordTypeInfo;
        }

        internal ComposableRecordTypeInfo GetMixinRequestingEntityRecordTypeInfo(
            ComposableRecordTypeInfo repositoryRecordTypeInfo)
        {
            if (repositoryRecordTypeInfo == null) throw new ArgumentNullException(nameof(repositoryRecordTypeInfo));
            if (!repositoryRecordTypeInfo.IsMixin) throw new ApplicationException("!repositoryRecordTypeInfo.IsMixin");
            var currentlyHandledEntityRecordType = CurrentlyHandledEntityRecordType.Value
                ?? Provider
                    .GetRequiredService<IScopedUseCaseHandledEntityRecordTypeProvider>()
                    .GetCurrentlyHandledEntityRecordType();
            var singleMixinApplicableEntityRecordType = repositoryRecordTypeInfo.ComposableTypeInfo.MixinToSingleRoot?.Type;
            var entityRecordType = singleMixinApplicableEntityRecordType ?? currentlyHandledEntityRecordType ?? throw new ApplicationException("entityRecordType == null");
            var entityRecordTypeInfo = GetRecordTypeInfo(entityRecordType);
            if (!entityRecordTypeInfo.IsEntity) throw new ApplicationException("!entityRecordTypeInfo.IsEntity");
            if (!entityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(repositoryRecordTypeInfo.Type))
                throw new ApplicationException("!entityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(repositoryRecordTypeInfo.Type)");
            return entityRecordTypeInfo;
        }

        internal override RecordsBatchTransaction.WithRecords<TRecord> CreateTransaction<TRecord, TRecordData>(
            RecordsDataAccess.IStarting recordsDataAccess)
            where TRecord : class
            where TRecordData : class
        {
            var proxy = (ComposingRecordsDataAccessProxy)recordsDataAccess;
            return proxy.RepositoryRecordTypeInfo.Type == typeof(TRecord)
                ? new ComposableRecordsBatchTransaction.IteratingInParallel<TRecord, TRecordData>(this, proxy)
                : new ComposableRecordsBatchTransaction.IteratedInParallel<TRecord, TRecordData>(this, proxy);
        }

        internal static Type ReplaceCurrentlyHandledEntityRecordType(Type entityRecordType)
        {
            if (CurrentlyHandledEntityRecordType.Value == entityRecordType)
                return entityRecordType;

            var prevEntityRecordType = CurrentlyHandledEntityRecordType.Value;
            CurrentlyHandledEntityRecordType.Value = entityRecordType;
            return prevEntityRecordType;
        }
    }
}