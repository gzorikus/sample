using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;
using YourCompany.Configuration.EFCore.CollationAwareSorting;
using YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking;
using YourCompany.OLTP.StateOwnership.Reflection.EFCore;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.CollationAwareSorting
{
    internal class EntityEntryModifyingVisitor<TRecordData, TQueryableRecordData>
        : EntityEntryPropertiesCopyingVisitor.Generic<TQueryableRecordData>
        where TRecordData : class
        where TQueryableRecordData : class, TRecordData
    {
        private readonly ICollationAwareModelProvider _collationAwareModelProvider;
        private Dictionary<object, ModifyingSpecificationItem> _modifyingSpecifications;
        private EFChangeTrackerTrackGraphStrategy _ensureUnchangedBeforeModifying;

        internal EntityEntryModifyingVisitor(ICollationAwareModelProvider collationAwareModelProvider)
            => _collationAwareModelProvider = collationAwareModelProvider ?? throw new ArgumentNullException(nameof(collationAwareModelProvider));

        internal void AddEachRecordModifyingSpecification(
            object specification, RecordDataQueryBuilder.ForSingleRecordData<TRecordData> queryBuilder)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (queryBuilder == null) throw new ArgumentNullException(nameof(queryBuilder));
            _modifyingSpecifications ??= new Dictionary<object, ModifyingSpecificationItem>();
            _modifyingSpecifications.Add(
                specification, new ModifyingSpecificationItem { QueryBuilder = queryBuilder, EachRecord = true });
        }

        internal void AddSpecifiedRecordModifyingSpecification(
            int recordIndex, object specification, RecordDataQueryBuilder.ForSingleRecordData<TRecordData> queryBuilder)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (queryBuilder == null) throw new ArgumentNullException(nameof(queryBuilder));

            if (_modifyingSpecifications == null
                || !_modifyingSpecifications.TryGetValue(specification, out var modifyingSpecification))
            {
                _modifyingSpecifications ??= new Dictionary<object, ModifyingSpecificationItem>();
                _modifyingSpecifications.Add(specification, modifyingSpecification = new ModifyingSpecificationItem
                {
                    QueryBuilder = queryBuilder,
                    EachRecord = false
                });
            }

            if (modifyingSpecification.EachRecord) throw new ApplicationException("modifyingSpecification.EachRecord");
            if (modifyingSpecification.RecordIndecies == null)
            {
                if (modifyingSpecification.SingleRecordIndex.HasValue)
                {
                    if (modifyingSpecification.SingleRecordIndex.Value == recordIndex)
                        throw new ApplicationException("modifyingSpecification.SingleRecordIndex.Value == recordIndex");

                    _modifyingSpecifications[specification] = modifyingSpecification with
                    {
                        RecordIndecies = new HashSet<int> { modifyingSpecification.SingleRecordIndex.Value, recordIndex },
                        SingleRecordIndex = null
                    };
                }
            }
            else
            {
                if (modifyingSpecification.SingleRecordIndex.HasValue) throw new ApplicationException("modifyingSpecification.SingleRecordIndex.HasValue");
                if (!modifyingSpecification.RecordIndecies.Add(recordIndex)) throw new ApplicationException("!modifyingSpecification.RecordIndecies.Add(recordIndex)");
            }
        }

        internal virtual PropertyEntry<TQueryableRecordData, long> ApplyChanges(
            int recordIndex, EntityEntryModifyingPair<TQueryableRecordData> modifyingEntriesPair, bool ensureUnchangedBeforeModifying)
        {
            var unchangedEntry = modifyingEntriesPair.LockedOrCreatedRecordData
                ?? throw new ApplicationException("modifyingEntriesPair.LockedOrCreatedRecordData == null");

            var propertiesChangingEntry
                = modifyingEntriesPair.DetachedRecordDataForSettingChangedProperties
                    ?? throw new ApplicationException("modifyingEntriesPair.DetachedRecordDataForSettingChangedProperties == null");

            if (ensureUnchangedBeforeModifying) EnsureUnchanged(unchangedEntry);
            ApplyModifyingSpecifications(recordIndex, unchangedEntry);
            if (ensureUnchangedBeforeModifying) unchangedEntry.DetectChanges();
            if (ensureUnchangedBeforeModifying) EnsureNoModifiedEntitiesUsedForSettingProperties(propertiesChangingEntry);
            SetChangedScalarProperties(propertiesChangingEntry, unchangedEntry);
            SetChangedNavigationProperties(propertiesChangingEntry, unchangedEntry, ensureUnchangedBeforeModifying);
            return unchangedEntry.GetNeverGeneratedSingleColumnBigintPrimaryKey();
        }

        internal void ApplyModifyingSpecifications(int recordIndex, EntityEntry<TQueryableRecordData> unchangedEntry)
        {
            if (unchangedEntry == null) throw new ArgumentNullException(nameof(unchangedEntry));
            if (_modifyingSpecifications == null) return;
            foreach (var kvp in _modifyingSpecifications)
            {
                object specification = kvp.Key;
                var modifyingSpecification = kvp.Value;
                if (modifyingSpecification.CheckToApplyTo(recordIndex))
                {
                    var queryBuilder = modifyingSpecification.QueryBuilder ?? throw new ApplicationException("queryBuilder == null");
                    queryBuilder.ChangeToMatch(unchangedEntry.Entity, specification);
                }
            }
        }

        internal void SetChangedScalarProperties(
            EntityEntry<TQueryableRecordData> propertiesChangingEntry, EntityEntry<TQueryableRecordData> unchangedEntry)
        {
            if (propertiesChangingEntry == null) throw new ArgumentNullException(nameof(propertiesChangingEntry));
            if (unchangedEntry == null) throw new ArgumentNullException(nameof(unchangedEntry));
            var entityTypeTopology = _collationAwareModelProvider.GetEntityTypeTopology(
                SortingKeyTopology.PropertiesOwner.From(unchangedEntry.Metadata));
            CopyNonDefault(entityTypeTopology, propertiesChangingEntry, unchangedEntry);
        }

        internal void SetChangedNavigationProperties(
            EntityEntry propertiesChangingEntry, EntityEntry unchangedEntry, bool ensureUnchangedBeforeModifying)
        {
            if (propertiesChangingEntry == null) throw new ArgumentNullException(nameof(propertiesChangingEntry));
            if (unchangedEntry == null) throw new ArgumentNullException(nameof(unchangedEntry));

            foreach (var possiblySetNavigation in propertiesChangingEntry.Navigations)
            {
                if (possiblySetNavigation.CurrentValue != null)
                {
                    var referenceEntry = possiblySetNavigation is not ReferenceEntry;
                    var ownedCollectionEntry = possiblySetNavigation as CollectionEntry;

                    if (possiblySetNavigation is not ReferenceEntry)
                    {
                        if (possiblySetNavigation is not CollectionEntry collectionEntry)
                            throw new ApplicationException("possiblySetNavigation is not ReferenceEntry && possiblySetNavigation is not CollectionEntry");
                        if (!collectionEntry.Metadata.TargetEntityType.IsOwned())
                            throw new ApplicationException("!collectionEntry.Metadata.TargetEntityType.IsOwned()");
                    }

                    var unchangedNavigation = unchangedEntry.Navigation(possiblySetNavigation.Metadata.Name);
                    if (ensureUnchangedBeforeModifying) EnsureUnchanged(unchangedNavigation);
                    unchangedNavigation.CurrentValue = possiblySetNavigation.CurrentValue;
                }
            }
        }

        protected void EnsureUnchanged(EntityEntry unchangedEntry)
        {
            if (unchangedEntry == null) throw new ArgumentNullException(nameof(unchangedEntry));
            if (!unchangedEntry.Context.ChangeTracker.AutoDetectChangesEnabled)
                throw new ApplicationException("!unchangedEntry.Context.ChangeTracker.AutoDetectChangesEnabled");

            _ensureUnchangedBeforeModifying ??= new EFChangeTrackerTrackGraphStrategy();
            _ensureUnchangedBeforeModifying.EnsureUnchanged(unchangedEntry);
        }

        protected void EnsureNoModifiedEntitiesUsedForSettingProperties(EntityEntry propertiesChangingEntry)
        {
            if (propertiesChangingEntry == null) throw new ArgumentNullException(nameof(propertiesChangingEntry));
            if (propertiesChangingEntry.State != EntityState.Detached) throw new ApplicationException("propertiesChangingEntry.State != EntityState.Detached");
            if (!propertiesChangingEntry.Context.ChangeTracker.AutoDetectChangesEnabled)
                throw new ApplicationException("!propertiesChangingEntry.Context.ChangeTracker.AutoDetectChangesEnabled");

            _ensureUnchangedBeforeModifying ??= new EFChangeTrackerTrackGraphStrategy();
            _ensureUnchangedBeforeModifying.EnsureUnchangedOrDetached(propertiesChangingEntry);
        }

        protected void EnsureUnchanged(NavigationEntry unchangedNavigation)
        {
            if (unchangedNavigation == null) throw new ArgumentNullException(nameof(unchangedNavigation));
            if (unchangedNavigation.IsModified) throw new ApplicationException("unchangedNavigation.IsModified");
            if (!unchangedNavigation.EntityEntry.Context.ChangeTracker.AutoDetectChangesEnabled)
                throw new ApplicationException("!unchangedNavigation.EntityEntry.Context.ChangeTracker.AutoDetectChangesEnabled");

            _ensureUnchangedBeforeModifying ??= new EFChangeTrackerTrackGraphStrategy();
            _ensureUnchangedBeforeModifying.EnsureUnchanged(unchangedNavigation);
        }

        private readonly struct ModifyingSpecificationItem
        {
            internal RecordDataQueryBuilder.ForSingleRecordData<TRecordData> QueryBuilder { get; init; }
            internal bool EachRecord { get; init; }
            internal int? SingleRecordIndex { get; init; }
            internal HashSet<int> RecordIndecies { get; init; }

            internal readonly bool CheckToApplyTo(int recordIndex)
                => EachRecord
                || SingleRecordIndex.HasValue && SingleRecordIndex.Value == recordIndex
                || (RecordIndecies?.Contains(recordIndex) ?? throw new ApplicationException("!SingleRecordIndex.HasValue && RecordIndecies == null"));
        }
    }
}