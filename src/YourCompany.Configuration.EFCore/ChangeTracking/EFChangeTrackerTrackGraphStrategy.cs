using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.ChangeTracking
{
    public class EFChangeTrackerTrackGraphStrategy : HashSet<EntityEntry>
    {
        private static readonly Func<EntityEntryGraphNode<HashSet<EntityEntry>>, bool>
            EnsureUnchangedCallback = unchangedNode =>
            {
                if (unchangedNode.Entry.State != EntityState.Unchanged) throw new ApplicationException("unchangedNode.Entry.State != EntityState.Unchanged");
                return ContinueTraversing(unchangedNode);
            };

        private static readonly Func<EntityEntryGraphNode<HashSet<EntityEntry>>, bool>
            EnsureUnchangedOrDetachedCallback = unchangedOrDetachedNode =>
            {
                if (unchangedOrDetachedNode.Entry.State != EntityState.Detached
                    && unchangedOrDetachedNode.Entry.State != EntityState.Unchanged)
                    throw new ApplicationException("unchangedOrDetachedNode.Entry.State != EntityState.Detached && unchangedOrDetachedNode.Entry.State != EntityState.Unchanged");
                return ContinueTraversing(unchangedOrDetachedNode);
            };

        public void EnsureUnchanged(EntityEntry unchangedEntry)
        {
            if (unchangedEntry == null) throw new ArgumentNullException(nameof(unchangedEntry));
            TrackGraphWithVisitedNodesState(unchangedEntry, EnsureUnchangedCallback);
        }

        public void EnsureUnchangedOrDetached(EntityEntry unchangedOrDetachedEntry)
        {
            if (unchangedOrDetachedEntry == null) throw new ArgumentNullException(nameof(unchangedOrDetachedEntry));
            TrackGraphWithVisitedNodesState(unchangedOrDetachedEntry, EnsureUnchangedOrDetachedCallback);
        }

        public void EnsureUnchanged(NavigationEntry unchangedNavigation)
        {
            if (unchangedNavigation == null) throw new ArgumentNullException(nameof(unchangedNavigation));
            TrackGraphWithVisitedNodesState(unchangedNavigation, EnsureUnchangedCallback);
        }

        public void EnsureUnchangedOrDetached(NavigationEntry unchangedOrDetachedNavigation)
        {
            if (unchangedOrDetachedNavigation == null) throw new ArgumentNullException(nameof(unchangedOrDetachedNavigation));
            TrackGraphWithVisitedNodesState(unchangedOrDetachedNavigation, EnsureUnchangedOrDetachedCallback);
        }

        public void TrackGraphWithVisitedNodesState(
            EntityEntry entry, Func<EntityEntryGraphNode<HashSet<EntityEntry>>, bool> callback)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            Clear();
            entry.Context.ChangeTracker.TrackGraph(entry, state: this, callback);
        }

        public void TrackGraphWithVisitedNodesState(
            NavigationEntry entry, Func<EntityEntryGraphNode<HashSet<EntityEntry>>, bool> callback)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (entry is ReferenceEntry referenceEntry)
            {
                if (referenceEntry?.TargetEntry != null)
                {
                    Clear();
                    Add(referenceEntry.EntityEntry);
                    entry.EntityEntry.Context.ChangeTracker.TrackGraph(referenceEntry.TargetEntry, state: this, callback);
                }
            }
            else if (entry is CollectionEntry collectionEntry)
            {
                if (collectionEntry?.CurrentValue != null)
                {
                    foreach (var relatedEntity in collectionEntry.CurrentValue)
                    {
                        if (relatedEntity == null) continue;
                        var relatedEntry = entry.EntityEntry.Context.Entry(relatedEntity)
                            ?? throw new ApplicationException("relatedEntity.Context.Entry(relatedEntity) == null");

                        Clear();
                        Add(collectionEntry.EntityEntry);
                        entry.EntityEntry.Context.ChangeTracker.TrackGraph(relatedEntry, state: this, callback);
                    }
                }
            }
            else
            {
                throw new ApplicationException("entry is not ReferenceEntry && entry is not CollectionEntry");
            }
        }

        protected static bool ContinueTraversing(EntityEntryGraphNode<HashSet<EntityEntry>> visitedNode)
            => ContinueTraversing(visitedNode, visitedNode?.NodeState);

        protected static bool ContinueTraversing<T>(EntityEntryGraphNode<T> visitedNode, HashSet<EntityEntry> visitedNodes)
        {
            if (visitedNode == null) throw new ArgumentNullException(nameof(visitedNode));
            if (visitedNodes == null) throw new ArgumentNullException(nameof(visitedNodes));
            return visitedNodes.Add(visitedNode.Entry);
        }
    }
}