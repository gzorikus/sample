using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        public abstract class SpecifiedRecordIncompatible : RecordsBatchTransactionSpecification
        {
            private SpecifiedRecordIncompatible() { }

            public override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                => base.ValidateCompatibilityWith(other)
                && !(other is ReadOnlyIncompatible.SpecifiedRecord)
                && !(other is Sorting.ByIds);

            public abstract class Paginate : SpecifiedRecordIncompatible
            {
                public int? ReducedBatchSize { get; }
                public int RecordsCountToSkip { get; }

                private Paginate(int? reducedBatchSize, int recordsCountToSkip)
                {
                    if (reducedBatchSize < 1) throw new ArgumentOutOfRangeException(nameof(reducedBatchSize), reducedBatchSize, message: null);
                    if (recordsCountToSkip < 0) throw new ArgumentOutOfRangeException(nameof(recordsCountToSkip), recordsCountToSkip, message: null);
                    ReducedBatchSize = reducedBatchSize;
                    RecordsCountToSkip = recordsCountToSkip;
                }

                public override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && !(other is Paginate);

                public sealed class Full : Paginate
                {
                    public Full(int reducedBatchSize, int recordsCountToSkip) : base(reducedBatchSize, recordsCountToSkip) { }
                }

                public sealed class ReduceBatchSize : Paginate
                {
                    public static ReduceBatchSize Single { get; } = new ReduceBatchSize(1);
                    public ReduceBatchSize(int batchSize) : base(batchSize, recordsCountToSkip: 0) { }
                }

                public sealed class SkipRecords : Paginate
                {
                    public SkipRecords(int recordsCountToSkip) : base(reducedBatchSize: null, recordsCountToSkip) { }
                }
            }
        }
    }
}