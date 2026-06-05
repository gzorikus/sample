using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntrySortingKeyCreatingVisitor
    {
        internal class Generic<TEntity> : EntityEntrySortingKeyCreatingVisitor where TEntity : class
        {
            internal EntityEntry<TEntity> PropertiesOwner { get; private set; }

            internal SortingKey CreateForSingleEntityQuery(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry<TEntity> propertiesOwner,
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

            internal Generic<TEntity> UseForSingleEntityQuery(EntityEntry<TEntity> propertiesOwner, SortingKey prefix = null)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                if (prefix == null) UseForNewSortingKey();
                if (prefix != null) UseForPropertiesAdding(prefix);
                return this;
            }

            protected override void CreateSortingKey<TValue>(SortingKeyTopology.ILastProperty topology)
                => CreateSortingKey<TValue>(
                    topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);

            protected override void CreateSortingKey<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
                => CreateSortingKey<TValue, TModelValue>(
                    topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);

            protected override TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName)
            {
                var entry = (EntityEntry<TEntity>)propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                return entry.Property<TProperty>(propertyName).CurrentValue;
            }
        }
    }
}