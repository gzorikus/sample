using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.RecordsManagement.UseCases;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal abstract partial class RecordsBatchTransaction<TRecord, TRecordData>
        : RecordsBatchTransactionDataBridge<TRecord, TRecordData>
        where TRecord : class
        where TRecordData : class
    {
        protected RecordsBatchTransaction(RecordsDataAccess.IStarting recordsDataAccess) : base(recordsDataAccess) { }

        protected override RecordsBatchTransactionCallback.ExtraInterfaceProvidersList CollectExtraInterfaceProviders()
        {
            if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
            CurrentRunState.EnsureIsAfterConfiguration();
            CurrentRunState.EnsureIsBeforeReturning();
            var extraInterfaceProviders = EnumerateRecordsBatchTransactionCallbackExtraInterfacesProviderIfAny();
            var extraInterfaceProvidersList = extraInterfaceProviders != null
                ? new RecordsBatchTransactionCallback.ExtraInterfaceProvidersList(this, extraInterfaceProviders)
                : new RecordsBatchTransactionCallback.ExtraInterfaceProvidersList(this);
            if (this is RecordsBatchTransactionCallback.IExtraInterfacesProvider extraInterfacesProvider)
                extraInterfaceProvidersList.Add(extraInterfacesProvider);
            return extraInterfaceProvidersList;
        }

        protected virtual IEnumerable<RecordsBatchTransactionCallback.IExtraInterfacesProvider>
            EnumerateRecordsBatchTransactionCallbackExtraInterfacesProviderIfAny() => null;

        protected override void PrepareRecordsBatchChanges()
        {
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            CurrentRunState.EnsureIsRecordsBuildingStarted();
            EnumerateRecordsBatchTransactionChangePreparersWithoutRecordsIfAny()?
                .PrepareRecordsBatchBeforeDataChangingAssertionsWithoutRecords(this);
            EnumerateRecordsBatchTransactionChangePreparersWithRecordsIfAny()?
                .PrepareRecordsBatchBeforeDataChangingAssertionsWithoutRecords(this);
        }

        protected virtual IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer>
            EnumerateRecordsBatchTransactionChangePreparersWithoutRecordsIfAny() => null;

        protected virtual IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer<TRecord>>
            EnumerateRecordsBatchTransactionChangePreparersWithRecordsIfAny() => null;

        protected override Task Return(CancellationToken cancellationToken, Exception runException = null)
        {
            if (_awaitTasksList?.Count > 0) throw new ApplicationException("_awaitTasksList?.Count > 0");
            _awaitTasksList = null;
            return base.Return(cancellationToken, runException);
        }
    }
}