using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal class ScopedRepository<TRecord> : IRepository<TRecord> where TRecord : class
    {
        private readonly ScopedRecordsBatchTransactionFactory _recordsBatchTransactionFactory;
        private readonly IRecordsDataAccessProvider _dataAccessProvider;

        public ScopedRepository(
            ScopedRecordsBatchTransactionFactory recordsBatchTransactionFactory,
            IRecordsDataAccessProvider dataAccessProvider)
        {
            _recordsBatchTransactionFactory = recordsBatchTransactionFactory ?? throw new ArgumentNullException(nameof(recordsBatchTransactionFactory));
            _dataAccessProvider = dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
        }

        public async Task<RecordsBatchTransaction.ISpecifiedRun<TRecord>> AccessRecordsBatch(CancellationToken cancellationToken)
        {
            var recordTypeInfo = _recordsBatchTransactionFactory.GetRecordTypeInfo(typeof(TRecord));
            var recordsDataAccess = await _dataAccessProvider.Connect(recordTypeInfo.RecordDataType, cancellationToken);
            try
            {
                return _recordsBatchTransactionFactory.CreateTransaction<TRecord>(recordTypeInfo, recordsDataAccess);
            }
            catch
            {
                await recordsDataAccess.Dispose(cancellationToken);
                throw;
            }
        }
    }
}