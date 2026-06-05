using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public interface IRepository<TRecord> where TRecord : class
    {
        Task<RecordsBatchTransaction.ISpecifiedRun<TRecord>> AccessRecordsBatch(CancellationToken cancellationToken);
    }
}