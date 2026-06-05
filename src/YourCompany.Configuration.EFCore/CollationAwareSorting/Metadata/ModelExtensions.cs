using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata
{
    public static class ModelExtensions
    {
        public static IEntityType FindEntityType(this IModel model, SortingKeyTopology.PropertiesOwner propertiesOwner)
        {
            var entityType = propertiesOwner.IsSharedEntityType
                ? model.FindEntityType(propertiesOwner.EntityName)
                : model.FindEntityType(propertiesOwner.EntityType);
            if (entityType != null && !propertiesOwner.Match(entityType))
                throw new ApplicationException("!propertiesOwner.Match(entityType)");
            return entityType ?? throw new ApplicationException("entityType == null");
        }

        public static SortingKeyTopology.ILastProperty GetEntityTypeTopology(
            this ICollationAwareModelProvider modelProvider, IEntityType entityType)
            => modelProvider.GetEntityTypeTopology(SortingKeyTopology.PropertiesOwner.From(entityType));
    }
}