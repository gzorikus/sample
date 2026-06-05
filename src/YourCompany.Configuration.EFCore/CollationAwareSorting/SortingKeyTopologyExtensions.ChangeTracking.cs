using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopologyExtensions
    {
        public static SortingKey CreateKeyForSingleEntityQueryOwner<TEntity>(
            this SortingKeyTopology.ILastProperty topology, EntityEntry<TEntity> propertiesOwner)
            where TEntity : class
            => new EntityEntrySortingKeyCreatingVisitor.Generic<TEntity>().CreateForSingleEntityQuery(topology, propertiesOwner);

        public static SortingKey CreateKeyForSingleEntityQueryOwner(
            this SortingKeyTopology.ILastProperty topology, EntityEntry propertiesOwner)
            => new EntityEntrySortingKeyCreatingVisitor.NonGeneric().CreateForSingleEntityQuery(topology, propertiesOwner);
    }
}