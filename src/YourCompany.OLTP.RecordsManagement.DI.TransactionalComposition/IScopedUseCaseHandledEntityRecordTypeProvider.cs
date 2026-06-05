using System;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    public interface IScopedUseCaseHandledEntityRecordTypeProvider
    {
        Type GetCurrentlyHandledEntityRecordType();
    }
}