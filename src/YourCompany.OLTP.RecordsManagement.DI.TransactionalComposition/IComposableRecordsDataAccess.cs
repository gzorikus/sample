using System;
using YourCompany.OLTP.RecordsManagement.Persistence;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    public interface IComposableRecordsDataAccess : RecordsDataAccess.IStarting
    {
        Type CurrentRecordDataType { get; set; }
        void EnsureRecordsDataLockingWithoutChanges(Type recordDataType);
        bool CheckHasLockedRecordsData(Type recordDataType);
    }
}