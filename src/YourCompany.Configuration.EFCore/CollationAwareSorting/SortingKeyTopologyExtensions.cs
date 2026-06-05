using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopologyExtensions
    {
        public static void EnsureCompatibleWithSingleEntityQueries(
            this SortingKeyTopology.ILastProperty topology,
            SortingKeyTopology.PropertiesOwner propertiesOwner,
            bool singleEntityReplacingQueriedSet)
        {
            bool matchedSinglePropertiesOwner = !topology.MultiplePropertyOwners
                && topology.PropertyOwner.MatchSingleTopology(propertiesOwner);

            if (!matchedSinglePropertiesOwner && !singleEntityReplacingQueriedSet)
                throw new ApplicationException("!matchedSinglePropertiesOwner && !singleEntityReplacingQueriedSet");

            if (singleEntityReplacingQueriedSet && !topology.UniquePropertyNames)
                throw new ApplicationException("singleEntityReplacingQueriedSet && !topology.UniquePropertyNames");
        }

        public static void EnsureCompatibleWithSingleEntityQueries(
            this SortingKeyTopology.ILastProperty topology,
            IReadOnlyTypeBase entityType,
            bool singleEntityReplacingQueriedSet)
        {
            bool matchedSinglePropertiesOwner = !topology.MultiplePropertyOwners
                && topology.PropertyOwner.Match(entityType);

            if (!matchedSinglePropertiesOwner && !singleEntityReplacingQueriedSet)
                throw new ApplicationException("!matchedSinglePropertiesOwner && !singleEntityReplacingQueriedSet");

            if (singleEntityReplacingQueriedSet && !topology.UniquePropertyNames)
                throw new ApplicationException("singleEntityReplacingQueriedSet && !topology.UniquePropertyNames");
        }

        public static void EnsureCompatibleWithSingleEntityQueries(
            this SortingKeyTopology.ILastProperty topology, Type entityType, bool singleEntityReplacingQueriedSet)
        {
            bool matchedUniqueEntityType = !topology.MultiplePropertyOwners && topology.PropertyOwner.MatchUnique(entityType);

            if (!matchedUniqueEntityType && !singleEntityReplacingQueriedSet)
                throw new ApplicationException("!matchedUniqueEntityType && !singleEntityReplacingQueriedSet");

            if (singleEntityReplacingQueriedSet && !topology.UniquePropertyNames)
                throw new ApplicationException("singleEntityReplacingQueriedSet && !topology.UniquePropertyNames");
        }

        public static void EnsureCompatibleWithSingleEntityQueries(
            this SortingKeyTopology.ILastProperty topology, bool singleEntityReplacingQueriedSet)
        {
            if (topology.MultiplePropertyOwners && !singleEntityReplacingQueriedSet)
                throw new ApplicationException("topology.MultiplePropertyOwners && !singleEntityReplacingQueriedSet");

            if (singleEntityReplacingQueriedSet && !topology.UniquePropertyNames)
                throw new ApplicationException("singleEntityReplacingQueriedSet && !topology.UniquePropertyNames");
        }
    }
}