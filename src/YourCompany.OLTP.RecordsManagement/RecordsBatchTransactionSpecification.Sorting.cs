using System;
using System.Collections.Generic;
using System.Linq;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        public abstract class Sorting : RecordsBatchTransactionSpecification
        {
            private Sorting() { }

            internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                => base.ValidateCompatibilityWith(other)
                && !(other is Sorting);

            public sealed class ByIds : Sorting
            {
                public IReadOnlyList<Identity> OrderedIdentities { get; }
                public bool SkipMissing { get; }

                internal ByIds(IReadOnlyList<Identity> orderedIdentities, bool skipMissing)
                {
                    OrderedIdentities = orderedIdentities ?? throw new ArgumentNullException(nameof(orderedIdentities));
                    SkipMissing = skipMissing;
                }

                internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && !(other is SpecifiedRecordIncompatible)
                    && (!(other is ReadOnlyIncompatible.SpecifiedRecord specifiedRecord)
                        || OrderedIdentities.Contains(
                            specifiedRecord.Identity, Identity.ReferenceComparer.AssignmentAgnostic.Instance));
            }

            public sealed class AfterId : Sorting
            {
                public Identity LastSortedIdentity { get; }

                internal AfterId(Identity lastSortedIdentity)
                    => LastSortedIdentity = lastSortedIdentity ?? throw new ArgumentNullException(nameof(lastSortedIdentity));

                internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && !(other is ReadOnlyIncompatible.SpecifiedRecord);
            }
        }
    }
}