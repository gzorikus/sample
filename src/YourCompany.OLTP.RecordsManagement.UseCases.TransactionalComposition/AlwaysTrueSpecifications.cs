using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    internal static class AlwaysTrueSpecifications
    {
        internal interface IIgnoredByPersistence { }

        internal sealed class EachRecordWithoutDataChanges<TRecord, TRecordData>
            : RecordsBatchTransactionSpecification
                .SpecificationToMatchWithoutDataChanges<TRecord, TRecordData>
                .SelfDeclaring,
            IIgnoredByPersistence
            where TRecord : class
            where TRecordData : class
        {
            internal static EachRecordWithoutDataChanges<TRecord, TRecordData> Instance { get; }
                = new EachRecordWithoutDataChanges<TRecord, TRecordData>();

            private EachRecordWithoutDataChanges() { }
            protected override bool Match(TRecordData recordData) => true;
        }

        internal sealed class EachRecordAfterDataChanging<TRecord, TRecordData>
            : RecordsBatchTransactionSpecification
                .ReadOnlyIncompatible
                .SpecificationToMatchAfterDataChanging<TRecord, TRecordData>
                .SelfDeclaring,
            IIgnoredByPersistence
            where TRecord : class
            where TRecordData : class
        {
            internal static EachRecordAfterDataChanging<TRecord, TRecordData> Instance { get; }
                = new EachRecordAfterDataChanging<TRecord, TRecordData>();

            private EachRecordAfterDataChanging() { }
            protected override bool Match(TRecordData recordData) => true;
        }

        internal sealed class SpecifiedRecordAfterDataChanging<TRecord, TRecordData>
            : RecordsBatchTransactionSpecification
                .ReadOnlyIncompatible
                .SpecifiedRecord
                .SpecificationToMatchAfterDataChanging<TRecord, TRecordData>
                .SelfDeclaring,
            IIgnoredByPersistence
            where TRecord : class
            where TRecordData : class
        {
            internal SpecifiedRecordAfterDataChanging(Identity identity) : base(identity) { }
            protected override bool Match(TRecordData recordData) => true;
        }
    }
}