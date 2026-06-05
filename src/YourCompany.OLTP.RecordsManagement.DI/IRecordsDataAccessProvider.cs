using System;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    public interface IRecordsDataAccessProvider
    {
        Task<RecordsDataAccess.IStarting> Connect(Type recordDataType, CancellationToken cancellationToken);
    }
}