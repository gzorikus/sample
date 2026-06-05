using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static class RecordsBatchTransactionSpecificationExtensions
    {
        public static ISpecification<TRecordData> Unwrap<TRecordData>(this ISpecification<TRecordData> specification)
            where TRecordData : class
            => specification is RecordsBatchTransactionSpecification.ISpecificationWrapper<TRecordData> wrapper
                ? wrapper.SpecificationToMatch ?? throw new ApplicationException("wrapper.SpecificationToMatchWithoutDataChanges == null")
                : specification;

        public static ISpecification<TRecordData> Unwrap<TRecordData>(
            this ISpecification<TRecordData> specification, Identity identity)
            where TRecordData : class
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord specifiedRecord
                && !specifiedRecord.Identity.CompareBy.Reference.Equals(identity))
                throw new ApplicationException("!specifiedRecord.Identity.CompareBy.Reference.Equals(identity)");

            return specification.Unwrap();
        }

        public static void EnsureMatches<TRecordData>(
            this IReadOnlyList<ISpecification<TRecordData>> specifications, Identity identity, TRecordData recordData)
            where TRecordData : class
        {
            for (int i = 0; i < specifications.Count; i++)
                if (!specifications[i].Match(recordData))
                    throw GetExceptionForSpecifiedRecordsMismatch(specifications[i], identity);
        }

        public static Exception GetExceptionForSpecifiedRecordsMismatch<TRecordData>(
            this ISpecification<TRecordData> specification, Identity identity)
            where TRecordData : class
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            return specification is RecordsBatchTransactionSpecification.ISpecificationWrapper<TRecordData> wrapper
                ? wrapper.GetExceptionForSpecifiedRecordsMismatch(identity)
                : new RecordsBatchTransactionSpecification
                    .RecordDataMismatchException<TRecordData>(identity, specification);
        }
    }
}