using System;
using System.Collections.Generic;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership;
using YourCompany.OLTP.StateOwnership.Reflection.DI;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal abstract partial class RecordsBatchTransaction<TRecord, TRecordData>
    {
        internal abstract partial class CurrentStateAccess
        {
            internal class Provider : CurrentStateAccess, ICurrentStateAccessProvider
            {
                internal Provider(
                    ScopedRecordsBatchTransactionFactory provider, RecordsDataAccess.IStarting recordsDataAccess)
                    : base(provider, recordsDataAccess)
                { }

                protected override IReadOnlyList<TRecord> CreateRecords(IReadOnlyList<Identity> identities)
                {
                    if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                    if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsRecordsBuildingStarted();

                    ScopedRecordsProvider.StartRecordCreation(this);
                    try
                    {
                        return base.CreateRecords(identities);
                    }
                    finally
                    {
                        ScopedRecordsProvider.StopRecordCreation(this);
                    }
                }

                protected override BuiltRecord BuildRecord(int recordsCount, int recordIndex, Identity identity)
                {
                    ScopedRecordsProvider.AssertCurrentStateAccessProvider(this);
                    return base.BuildRecord(recordsCount, recordIndex, identity);
                }

                ICurrentStateAccess ICurrentStateAccessProvider.GetCurrentStateAccess(Type recordDataType)
                {
                    if (recordDataType != typeof(TRecordData)) throw new ApplicationException("recordDataType != typeof(TRecordData)");
                    if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                    if (!_currentlyResolvedRecord.HasValue) throw new ApplicationException("!_currentlyResolvedRecord.HasValue");
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsRecordsBuildingStarted();
                    return this;
                }
            }
        }
    }
}