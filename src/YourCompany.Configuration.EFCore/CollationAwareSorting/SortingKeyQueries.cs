using System;
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
    }
}