using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public static class RecordsBatchTransaction
    {
        public interface ISpecified : IRecordsBatchSizeLimit
        {
            void For(RecordsBatchTransactionSpecification specification);
        }

        public interface ISpecifiedRun : ISpecified
        {
            Task<IRecordsBatch> Run(CancellationToken cancellationToken);
        }

        public interface ISpecifiedRun<TRecord> : ISpecifiedRun where TRecord : class
        {
            new Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken);
        }
    }
}