using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal sealed partial class OrElseEqualityKeys : List<SortingKey>
    {
        private bool _protectedFromChanges;

        public ICollationAwareModelProvider ModelProvider => Count > 0
            ? this[0]?.ModelProvider ?? throw new ApplicationException("this[0]?.ModelProvider == null")
            : throw new ApplicationException("Count == 0");

        public SortingKeyTopology.ILastProperty PossibleSingleTopology { get; private set; }

        internal OrElseEqualityKeys() { }
        internal OrElseEqualityKeys(int capacity) : base(capacity) { }
        internal OrElseEqualityKeys(IEnumerable<SortingKey> collection) : base(collection) => RedetermineSingleTopology();

        internal void AddAndTryRetainSingleTopology(SortingKey item)
        {
            if (_protectedFromChanges) throw new ApplicationException("_protectedFromChanges");
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.ReplaceQueriedSetName != null) throw new ApplicationException("item.ReplaceQueriedSetName != null");

            Add(item);
            if (Count == 1)
            {
                PossibleSingleTopology = this[0];
            }
            else if (PossibleSingleTopology != null)
            {
                PossibleSingleTopology = SortingKeyTopology.Comparer.Instance.Equals(PossibleSingleTopology, item)
                    ? PossibleSingleTopology
                    : null;
            }
        }

        internal void RedetermineSingleTopology()
        {
            SortingKeyTopology.ILastProperty singleTopology = Count > 0 ? this[0] : null;
            for (int i = 0; i < Count; i++)
            {
                var item = this[i] ?? throw new ApplicationException("this[i] == null");
                if (item.ReplaceQueriedSetName != null) throw new ApplicationException("item.ReplaceQueriedSetName != null");

                if (singleTopology != null)
                {
                    singleTopology = SortingKeyTopology.Comparer.Instance.Equals(singleTopology, item)
                        ? singleTopology
                        : null;
                }
            }
            PossibleSingleTopology = singleTopology;
        }

        internal void ProtectFromChanges()
        {
            if (_protectedFromChanges) throw new ApplicationException("_protectedFromChanges");
            _protectedFromChanges = true;
        }
    }
}