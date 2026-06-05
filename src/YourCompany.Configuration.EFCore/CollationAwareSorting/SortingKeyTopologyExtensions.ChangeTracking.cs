using System;
using System.Collections.Generic;
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

        public static SortingKey CreateKeyForMultiEntityQueryOwners(
            this SortingKeyTopology.ILastProperty topology, IReadOnlyList<EntityEntry> propertyOwners)
            => new EntityEntrySortingKeyCreatingVisitor.NonGeneric().CreateForMultiEntityQuery(topology, propertyOwners);

        internal static bool TryGetSingleMatchingOwner(
            this SortingKeyTopology.PropertiesOwner matchTo,
            IReadOnlyList<EntityEntry> propertyOwners,
            out EntityEntry singleEntry)
        {
            if (propertyOwners == null) throw new ArgumentNullException(nameof(propertyOwners));
            if (propertyOwners.Count == 0) throw new ApplicationException("propertyOwners.Count == 0");

            singleEntry = null;
            for (int i = 0; i < propertyOwners.Count; i++)
            {
                var entry = propertyOwners[i];
                var entityType = entry?.Metadata ?? throw new ApplicationException("entry?.Metadata == null");
                if (matchTo.Match(entityType))
                {
                    if (singleEntry != null) throw new ApplicationException("singleEntry != null");
                    singleEntry = entry;
                }
            }

            return singleEntry != null;
        }
    }
}