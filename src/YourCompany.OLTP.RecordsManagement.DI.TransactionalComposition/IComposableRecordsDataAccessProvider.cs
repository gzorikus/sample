using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    public interface IComposableRecordsDataAccessProvider
    {
        Task<IComposableRecordsDataAccess> Connect(Type entityRecordDataType, CancellationToken cancellationToken);
    }
}