using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    public abstract partial class EntityEntrySortingKeyCreatingVisitor
    {
        public class NonGeneric : EntityEntrySortingKeyCreatingVisitor
        {
            public EntityEntry PropertiesOwner { get; private set; }
            public IReadOnlyList<EntityEntry> PropertyOwners { get; private set; }

            public SortingKey CreateForSingleEntityQuery(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry propertiesOwner,
                SortingKey prefix = null,
                string replaceQueriedSetName = null)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));

                topology.VisitPrefixFirst(UseForSingleEntityQuery(propertiesOwner, prefix));
                var sortingKey = Prefix ?? throw new ApplicationException("Prefix == null");

                replaceQueriedSetName ??= LastSortingKeyReplacingQueriedSetName;
                sortingKey.EnsureCompatibleWithSingleEntityQueries(
                    propertiesOwner.Metadata, singleEntityReplacingQueriedSet: replaceQueriedSetName != null);
                sortingKey.ReplaceQueriedSetName = replaceQueriedSetName;
                if (sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null)
                    throw new ApplicationException("sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null");

                return sortingKey;
            }

            public NonGeneric UseForSingleEntityQuery(EntityEntry propertiesOwner, SortingKey prefix = null)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                PropertyOwners = null;
                if (prefix == null) UseForNewSortingKey();
                if (prefix != null) UseForPropertiesAdding(prefix);
                return this;
            }

            public SortingKey CreateForMultiEntityQuery(
                SortingKeyTopology.ILastProperty topology,
                IReadOnlyList<EntityEntry> propertyOwners,
                SortingKey prefix = null)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                topology.VisitPrefixFirst(UseForMultiEntityQuery(propertyOwners, prefix));
                return Prefix ?? throw new ApplicationException("Prefix == null");
            }

            public NonGeneric UseForMultiEntityQuery(IReadOnlyList<EntityEntry> propertyOwners, SortingKey prefix = null)
            {
                PropertyOwners = propertyOwners ?? throw new ArgumentNullException(nameof(propertyOwners));
                PropertiesOwner = null;
                if (prefix == null) UseForNewSortingKey();
                if (prefix != null) UseForPropertiesAdding(prefix);
                return this;
            }

            protected override void CreateSortingKey<TValue>(SortingKeyTopology.ILastProperty topology)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));

                if (PropertiesOwner != null)
                {
                    if (PropertyOwners != null) throw new ApplicationException("PropertiesOwner != null && PropertyOwners != null");
                    Prefix = CreateSortingKey<TValue>(
                        topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);
                }
                else if (PropertyOwners != null)
                {
                    if (!topology.PropertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry))
                        throw new ApplicationException("!topology.PropertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry)");

                    Prefix = CreateSortingKey<TValue>(
                        topology.PropertyOwner, topology.PropertyName, topology.Descending, entry);
                }
                else
                {
                    throw new ApplicationException("PropertiesOwner == null && PropertyOwners == null");
                }
            }

            protected override void CreateSortingKey<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));

                if (PropertiesOwner != null)
                {
                    if (PropertyOwners != null) throw new ApplicationException("PropertiesOwner != null && PropertyOwners != null");
                    Prefix = CreateSortingKey<TValue, TModelValue>(
                        topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);
                }
                else if (PropertyOwners != null)
                {
                    if (!topology.PropertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry))
                        throw new ApplicationException("!topology.PropertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry)");

                    Prefix = CreateSortingKey<TValue, TModelValue>(
                        topology.PropertyOwner, topology.PropertyName, topology.Descending, entry);
                }
                else
                {
                    throw new ApplicationException("PropertiesOwner == null && PropertyOwners == null");
                }
            }

            protected override TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName)
            {
                if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));
                var entryEntityType = propertiesOwner.Metadata.ClrType ?? throw new ApplicationException("propertiesOwner.Metadata.ClrType == null");
                var getter = EFEntityEntryPropertiesCache<TProperty>.GetGetter(entryEntityType, propertyName);
                return getter(propertiesOwner);
            }
        }
    }
}