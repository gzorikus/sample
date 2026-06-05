using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntrySortingKeyCreatingVisitor
    {
        internal class NonGeneric : EntityEntrySortingKeyCreatingVisitor
        {
            internal EntityEntry PropertiesOwner { get; private set; }

            internal SortingKey CreateForSingleEntityQuery(
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

                return sortingKey;
            }

            public NonGeneric UseForSingleEntityQuery(EntityEntry propertiesOwner, SortingKey prefix = null)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                if (prefix == null) UseForNewSortingKey();
                if (prefix != null) UseForPropertiesAdding(prefix);
                return this;
            }

            protected override void CreateSortingKey<TValue>(SortingKeyTopology.ILastProperty topology)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                Prefix = CreateSortingKey<TValue>(
                    topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);
            }

            protected override void CreateSortingKey<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                Prefix = CreateSortingKey<TValue, TModelValue>(
                    topology.PropertyOwner, topology.PropertyName, topology.Descending, PropertiesOwner);
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