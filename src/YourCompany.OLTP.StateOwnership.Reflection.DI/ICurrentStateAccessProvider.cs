using System;

namespace YourCompany.OLTP.StateOwnership.Reflection.DI
{
    internal interface ICurrentStateAccessProvider
    {
        ICurrentStateAccess GetCurrentStateAccess(Type recordDataType);
    }
}