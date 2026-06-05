using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        internal abstract partial class WithRecords<TRecord> : WithIdentities,
            ISpecifiedRun<TRecord>,
            IRecordsBatch<TRecord>
            where TRecord : class
        {
            protected sealed override Type RecordType => typeof(TRecord);
            public IReadOnlyList<TRecord> Records { get; private set; }

            async Task<IRecordsBatch<TRecord>> ISpecifiedRun<TRecord>.Run(CancellationToken cancellationToken)
            {
                await RunOnce(cancellationToken);
                CurrentRunState.EnsureIsReturning();
                return this;
            }

            protected sealed override void BuildRecords(IReadOnlyList<Identity> identities)
            {
                if (identities == null) throw new ArgumentNullException(nameof(identities));
                if (!ReferenceEquals(identities, Identities)) throw new ApplicationException("!ReferenceEquals(identities, Identities)");
                if (Records != null) throw new ApplicationException("Records != null");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                Records = CreateRecords(identities) ?? throw new ApplicationException("CreateRecords(identities) == null");
                if (this.CheckHasSkippedMissingRecords() != ByIdsSkipMissing) throw new ApplicationException("this.CheckHasSkippedMissingRecords() != ByIdsSkipMissing");
            }

            protected abstract IReadOnlyList<TRecord> CreateRecords(IReadOnlyList<Identity> identities);
        }
    }
}