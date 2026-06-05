using System;

namespace YourCompany.OLTP.StateOwnership.Reflection.EFCore
{
    public static class RecordDataQueries
    {
        public readonly struct PrimaryKeyWithExists
        {
            public Guid PublicKey { get; init; }
            public long PrivateKey { get; init; }
            public bool RecordDataExists { get; init; }
        }

        public readonly struct PrimaryKeyWithRecordData<TQueryableRecordData>
        {
            public Guid PublicKey { get; init; }
            public long PrivateKey { get; init; }
            public TQueryableRecordData RecordData { get; init; }
        }
    }
}