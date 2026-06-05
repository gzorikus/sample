using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract class SortingKeyPropertiesVisitor : SortingKey.IPrefixFirstPropertiesVisitor,
        SortingKeyQueries.IMultiTopologyQueryComposedSortingKeysVisitor
    {
        public ICollationAwareModelProvider ModelProvider { get; set; }

        public SortingKey CurrentSortingKey { get; private set; }
        public SortingKey CurrentSortingKeyPredecessor { get; private set; }
        public int CurrentSortingKeyIndex { get; private set; } = -1;
        public SortingKey CurrentSortingKeyProperty { get; private set; }
        public SortingKey CurrentSortingKeyPropertyPrefix { get; private set; }
        public int CurrentSortingKeyPropertyIndex { get; private set; } = -1;
        public int VisitedPropertiesTotal { get; private set; }

        public SortingKeyTopology.ILastProperty EnsureSingleTopology { get; set; }

        void SortingKeyQueries.IMultiTopologyQueryComposedSortingKeysVisitor.VisitSortingKey(SortingKey sortingKey)
        {
            var predecessor = SetNextSortingKey(sortingKey);
            try
            {
                VisitCurrentSortingKey(sortingKey);
                FinishCurrentSortingKeyVisit(sortingKey, predecessor);
            }
            catch
            {
                ResetVisitState();
                throw;
            }
        }

        void SortingKey.IPrefixFirstPropertiesVisitor.VisitProperty<TValue>(SortingKey.ValueHolding<TValue> sortingKey)
        {
            var currentSortingKey = CurrentSortingKey ?? throw new ApplicationException("CurrentSortingKey == null");
            var prefix = SetNextProperty(sortingKey);
            try
            {
                VisitCurrentProperty(sortingKey);
                FinishCurrentPropertyVisit(currentSortingKey, sortingKey, prefix);
            }
            catch
            {
                ResetVisitState();
                throw;
            }
        }

        void SortingKey.IPrefixFirstPropertiesVisitor.VisitProperty<TValue, TModelValue>(
            SortingKey.ValueHolding<TValue>.ConvertedFrom<TModelValue> sortingKey)
        {
            var currentSortingKey = CurrentSortingKey ?? throw new ApplicationException("CurrentSortingKey == null");
            var prefix = SetNextProperty(sortingKey);
            try
            {
                VisitCurrentProperty(sortingKey);
                FinishCurrentPropertyVisit(currentSortingKey, sortingKey, prefix);
            }
            catch
            {
                ResetVisitState();
                throw;
            }
        }

        protected virtual void VisitCurrentSortingKey(SortingKey sortingKey)
        {
            if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
            ValidateCurrentSortingKeyBeforeVisit(sortingKey);
            sortingKey.VisitPrefixFirst(this);
        }

        protected virtual void VisitCurrentProperty<TValue>(SortingKey.ValueHolding<TValue> property)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => VisitCurrentProperty(property, property.Value);

        protected virtual void VisitCurrentProperty<TValue, TModelValue>(
            SortingKey.ValueHolding<TValue>.ConvertedFrom<TModelValue> property)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => VisitCurrentProperty(property, property.Value);

        protected virtual void VisitCurrentProperty<TProperty>(SortingKey property, TProperty value)
            => ValidateCurrentPropertyBeforeVisit(property);

        public SortingKey SetNextSortingKey(SortingKey sortingKey)
        {
            if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
            if (CurrentSortingKey != null) throw new ApplicationException("CurrentSortingKey != null");
            if (CurrentSortingKeyProperty != null) throw new ApplicationException("CurrentSortingKeyProperty != null");
            if (CurrentSortingKeyPropertyPrefix != null) throw new ApplicationException("CurrentSortingKeyPropertyPrefix != null");
            if (CurrentSortingKeyPropertyIndex != -1) throw new ApplicationException("CurrentSortingKeyPropertyIndex != -1");

            if (EnsureSingleTopology != null
                && !SortingKeyTopology.Comparer.Instance.Equals(sortingKey, EnsureSingleTopology))
                throw new ApplicationException("!SortingKeyTopology.Comparer.Instance.Equals(sortingKey, EnsureSingleTopology)");

            var predecessor = CurrentSortingKeyPredecessor;
            if (predecessor != null && CurrentSortingKeyIndex < 1) throw new ApplicationException("predecessor != null && CurrentSortingKeyIndex < 1");

            CurrentSortingKey = sortingKey;
            CurrentSortingKeyIndex++;
            return CurrentSortingKeyPredecessor;
        }

        protected virtual void ValidateCurrentSortingKeyBeforeVisit(SortingKey sortingKey)
        {
            if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
            if (!ReferenceEquals(sortingKey, CurrentSortingKey)) throw new ApplicationException("!ReferenceEquals(sortingKey, CurrentSortingKey");
            if (CurrentSortingKeyProperty != null) throw new ApplicationException("CurrentSortingKeyProperty != null");
            if (CurrentSortingKeyPropertyPrefix != null) throw new ApplicationException("CurrentSortingKeyPropertyPrefix != null");
            if (CurrentSortingKeyPropertyIndex != -1) throw new ApplicationException("CurrentSortingKeyPropertyIndex != -1");
        }

        protected virtual void FinishCurrentSortingKeyVisit(SortingKey sortingKey, SortingKey predecessor)
        {
            if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
            if (!ReferenceEquals(sortingKey, CurrentSortingKey)) throw new ApplicationException("!ReferenceEquals(sortingKey, CurrentSortingKey)");
            if (!ReferenceEquals(sortingKey, CurrentSortingKeyPropertyPrefix)) throw new ApplicationException("!ReferenceEquals(sortingKey, CurrentSortingKeyPropertyPrefix)");
            if (!ReferenceEquals(predecessor, CurrentSortingKeyPredecessor)) throw new ApplicationException("!ReferenceEquals(predecessor, CurrentSortingKeyPredecessor)");
            if (CurrentSortingKeyProperty != null) throw new ApplicationException("CurrentSortingKeyProperty != null");
            CurrentSortingKey = null;
            CurrentSortingKeyPredecessor = sortingKey;
            CurrentSortingKeyPropertyPrefix = null;
            CurrentSortingKeyPropertyIndex = -1;
        }

        public SortingKey SetNextProperty(SortingKey property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (CurrentSortingKey == null) throw new ApplicationException("CurrentSortingKey == null");
            if (CurrentSortingKeyProperty != null) throw new ApplicationException("CurrentSortingKeyProperty != null");

            var prefix = CurrentSortingKeyPropertyPrefix;
            if (!ReferenceEquals(prefix, property.PrefixKey)) throw new ApplicationException("!ReferenceEquals(prefix, property.PrefixKey)");
            if (ReferenceEquals(prefix, CurrentSortingKey)) throw new ApplicationException("ReferenceEquals(prefix, CurrentSortingKey)");
            if (prefix != null && CurrentSortingKeyPropertyIndex < 1) throw new ApplicationException("prefix != null && CurrentSortingKeyPropertyIndex < 1");

            CurrentSortingKeyProperty = property;
            CurrentSortingKeyPropertyIndex++;
            return prefix;
        }

        protected virtual void ValidateCurrentPropertyBeforeVisit(SortingKey property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (CurrentSortingKey == null) throw new ApplicationException("CurrentSortingKey == null");
            if (ReferenceEquals(CurrentSortingKey, CurrentSortingKeyPropertyPrefix)) throw new ApplicationException("CurrentSortingKey, CurrentSortingKeyPropertyPrefix");
            if (!ReferenceEquals(property, CurrentSortingKeyProperty)) throw new ApplicationException("!ReferenceEquals(property, CurrentSortingKeyProperty)");
            if (CurrentSortingKeyPropertyIndex < 0) throw new ApplicationException("CurrentSortingKeyPropertyIndex < 0");
        }

        protected virtual void FinishCurrentPropertyVisit(SortingKey sortingKey, SortingKey property, SortingKey prefix)
        {
            if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (!ReferenceEquals(sortingKey, CurrentSortingKey)) throw new ApplicationException("!ReferenceEquals(sortingKey, CurrentSortingKey)");
            if (!ReferenceEquals(property, CurrentSortingKeyProperty)) throw new ApplicationException("!ReferenceEquals(property, CurrentSortingKeyProperty)");
            if (!ReferenceEquals(prefix, CurrentSortingKeyPropertyPrefix)) throw new ApplicationException("!ReferenceEquals(prefix, CurrentSortingKeyPropertyPrefix)");
            CurrentSortingKeyProperty = null;
            CurrentSortingKeyPropertyPrefix = property;
            VisitedPropertiesTotal++;
        }

        public virtual void ResetVisitState()
        {
            CurrentSortingKey = null;
            CurrentSortingKeyPredecessor = null;
            CurrentSortingKeyIndex = -1;
            CurrentSortingKeyProperty = null;
            CurrentSortingKeyPropertyPrefix = null;
            CurrentSortingKeyPropertyIndex = -1;
            VisitedPropertiesTotal = 0;
        }
    }
}