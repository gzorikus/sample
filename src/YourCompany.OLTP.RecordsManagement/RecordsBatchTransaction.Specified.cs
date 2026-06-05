using System;
using System.Collections.Generic;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class Specified : ISpecified
        {
            private List<RecordsBatchTransactionSpecification> _transactionSpecifications;

            public abstract int MaxRecordsInBatch { get; }

            public int SkippedRecords
                => ByIdsSkipMissing
                    ? ByIdsSkippedMissingRecords
                    : _recordsCountToSkip ?? throw new ApplicationException("!_recordsCountToSkip.HasValue");

            public int ByIdsSkippedMissingRecords => !ByIdsSkipMissing
                ? throw new ApplicationException("!ByIdsSkipMissing")
                : _skippedMissingRecords ?? throw new ApplicationException("!_skippedMissingRecords.HasValue");

            public IReadOnlyList<RecordsBatchTransactionSpecification> TransactionSpecifications
                => _transactionSpecifications ?? Array.Empty<RecordsBatchTransactionSpecification>()
                    as IReadOnlyList<RecordsBatchTransactionSpecification>;

            public void For(RecordsBatchTransactionSpecification specification)
            {
                if (!ValidateCompatibility(specification)) throw new ApplicationException("!ValidateCompatibility(specification)");
                if (!Add(specification)) throw new ApplicationException("!Add(specification)");
            }

            public bool ValidateCompatibility(RecordsBatchTransactionSpecification specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();
                if (_transactionSpecifications == null) return true;
                for (int i = 0; i < TransactionSpecifications.Count; i++)
                    if (!specification.ValidateCompatibilityWith(TransactionSpecifications[i]))
                        return false;
                return true;
            }

            public bool Add(RecordsBatchTransactionSpecification specification)
            {
                EnsureIsConfigurable();
                bool handled = Handle(specification);
                if (handled)
                {
                    _transactionSpecifications = _transactionSpecifications ?? new List<RecordsBatchTransactionSpecification>();
                    _transactionSpecifications.Add(specification);
                }
                return handled;
            }

            protected abstract void EnsureIsConfigurable();
        }
    }
}