using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransactionConfiguration<TRecord>
    {
        public readonly struct ById
        {
            private readonly RecordsBatchTransaction.ISpecifiedRun<TRecord> _transaction;
            private readonly OrderedIdentityUniqueReferencesList _orderedIdentities;

            internal ById(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
            {
                _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
                _orderedIdentities = new OrderedIdentityUniqueReferencesList();
            }

            public ById Then(Identity nextOrderedIdentity)
            {
                var identities = _orderedIdentities ?? throw new ApplicationException("identities == null");
                identities.AddUniqueReference(nextOrderedIdentity);
                return this;
            }

            public ByIds OrderingRecords(bool skipMissing = false)
            {
                var transaction = _transaction ?? throw new ApplicationException("transaction == null");
                var identities = _orderedIdentities ?? throw new ApplicationException("identities == null");
                if (identities.Count == 0) throw new ApplicationException("identities.Count == 0");

                identities.ProtectFromChanges();

                Identity single = null;
                HashSet<Identity> unique = null;

                if (identities.Count == 1)
                {
                    single = identities[0];
                }
                else if (identities.Count > 1)
                {
                    if (unique.Count != identities.Count) throw new ApplicationException("unique.Count != identities.Count");
                }

                transaction.For(new RecordsBatchTransactionSpecification.Sorting.ByIds(identities, skipMissing));
                return new ByIds(transaction, single, unique);
            }

            private sealed class OrderedIdentityUniqueReferencesList : List<Identity>
            {
                private HashSet<Identity> _uniqueIdentitiesInTransactionByReference;
                private bool _protectedFromChanges;

                internal void AddUniqueReference(Identity nextOrderedIdentity)
                {
                    if (nextOrderedIdentity == null) throw new ArgumentNullException(nameof(nextOrderedIdentity));
                    if (_protectedFromChanges) throw new ApplicationException("_protectedFromChanges");
                    if (Count == 1)
                    {
                        if (_uniqueIdentitiesInTransactionByReference != null) throw new ApplicationException("_uniqueIdentitiesInTransactionByReference != null");
                        _uniqueIdentitiesInTransactionByReference = new HashSet<Identity>(
                            Identity.ReferenceComparer.AssignmentAgnostic.Instance);
                        _uniqueIdentitiesInTransactionByReference.Add(nextOrderedIdentity);
                    }
                    else if (Count > 1)
                    {
                        HashSet<Identity> unique = _uniqueIdentitiesInTransactionByReference ?? throw new ApplicationException("unique == null");
                        if (unique.Count != Count) throw new ApplicationException("unique.Count != Count");
                        if (!unique.Add(nextOrderedIdentity)) throw new ApplicationException("!unique.Add(nextOrderedIdentity)");
                    }

                    Add(nextOrderedIdentity);
                }

                internal void ProtectFromChanges()
                {
                    if (_protectedFromChanges) throw new ApplicationException("_protectedFromChanges");
                    _protectedFromChanges = true;
                }
            }
        }
    }
}