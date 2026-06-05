namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        public abstract class ReadOnly : RecordsBatchTransactionSpecification
        {
            internal ReadOnly() { }

            internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                => base.ValidateCompatibilityWith(other)
                && !(other is ReadOnlyIncompatible);

            public sealed class IncludeRecords : ReadOnly
            {
                internal static IncludeRecords Instance { get; } = new IncludeRecords();
                private IncludeRecords() { }
            }
        }
    }
}