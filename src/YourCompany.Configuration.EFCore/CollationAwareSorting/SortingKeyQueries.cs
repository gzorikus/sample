using System;
using System.Collections.Generic;
using System.Linq;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyQueries
    {
        public interface ISingleEntityFiltering : ISingleEntitySorting, ISingleEntityEquality
        {
            IOrderedQueryable<TEntity> GetNext<TEntity>(IQueryable<TEntity> queryable) where TEntity : class;
            new IOrderedQueryable<TEntity> GetEqual<TEntity>(IQueryable<TEntity> queryable) where TEntity : class;
        }

        public interface ISingleEntitySorting : ISingleTopologySingleEntityQuery
        {
            IOrderedQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> queryable) where TEntity : class;
        }

        public interface ISingleEntityEquality : IMultiTopologySingleEntityQuery
        {
            void EnsureCompatibleWithSingleEntityEqualityQueries(Type entityType, bool singleEntityReplacingQueriedSet);
            IQueryable<TEntity> GetEqual<TEntity>(IQueryable<TEntity> queryable) where TEntity : class;
        }

        public interface ISingleTopologySingleEntityQuery : ISingleTopologyQuery, IMultiTopologySingleEntityQuery { }
        public interface IMultiTopologySingleEntityQuery : IMultiTopologyQuery, ISingleEntityQuery { }
        public interface ISingleTopologyMultiEntityQuery : ISingleTopologyQuery, IMultiTopologyMultiEntityQuery { }
        public interface IMultiTopologyMultiEntityQuery : IMultiTopologyQuery, IMultiEntityQuery { }

        public interface ISingleTopologyQuery : IMultiTopologyQuery
        {
            SortingKeyTopology.ILastProperty SingleTopology { get; }
        }

        public interface IMultiTopologyQuery : SortingKeyTopology.IKeyModel
        {
            SortingKeyTopology.ILastProperty PossibleSingleTopology { get; }
            int VisitComposed(IMultiTopologyQueryComposedSortingKeysVisitor visitor);
        }

        public interface IMultiTopologyQueryComposedSortingKeysVisitor
        {
            ICollationAwareModelProvider ModelProvider { get; set; }
            void VisitSortingKey(SortingKey sortingKey);
        }

        public interface ISingleEntityQuery : SortingKeyTopology.IKeyModel
        {
            string ReplaceQueriedSetName { get; }
        }

        public interface IMultiEntityQuery : SortingKeyTopology.IKeyModel
        {
            IReadOnlyList<MultiEntityQuerySortingKeyPropertyOwnerIndex> EntitiesValueTuplePropertyOwnerIndecies { get; }

            IQueryable<TEntity> FilterOwnedPropertiesOnlyForEquality<TEntity>(IQueryable<TEntity> queryable)
                where TEntity : class;
        }

#pragma warning disable CA2231 // Implement the equality operators and make their behavior identical to that of the Equals method
#pragma warning disable CS0659 // overrides Object.Equals(object o) but does not override Object.GetHashCode()
        public readonly struct MultiEntityQuerySortingKeyPropertyOwnerIndex
#pragma warning restore CA2231 // Implement the equality operators and make their behavior identical to that of the Equals method
#pragma warning restore CS0659 // overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            public SortingKeyTopology.ILastProperty PrefixFirstProperty { get; init; }
            public int EntitiesValueTupleParameterValueIndex { get; init; }
            public bool UseOwnerForSortingByThisProperty { get; init; }

            public void Deconstruct(
                out SortingKeyTopology.ILastProperty property, out int ownerIndex, out bool useForSorting)
            {
                property = PrefixFirstProperty;
                ownerIndex = EntitiesValueTupleParameterValueIndex;
                useForSorting = UseOwnerForSortingByThisProperty;
            }

            internal bool EqualsForQueriesCacheKeyOnly(MultiEntityQuerySortingKeyPropertyOwnerIndex other)
                => SortingKeyTopology.Comparer.Instance.Equals(PrefixFirstProperty, other.PrefixFirstProperty)
                && EntitiesValueTupleParameterValueIndex == other.EntitiesValueTupleParameterValueIndex
                && UseOwnerForSortingByThisProperty == other.UseOwnerForSortingByThisProperty;

            public override bool Equals(object obj) => throw new NotSupportedException(nameof(Equals));
        }
    }
}