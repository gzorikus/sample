using System;
using YourCompany.Persistence;

namespace YourCompany.Dapper
{
    internal class DapperAccountRecordData : IAccountRecordData
    {
        public decimal? Balance { get; set; }
        internal long? ResolvedId { get; set; }
        internal Guid? ResolvedPublicId { get; set; }
    }
}