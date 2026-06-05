using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata
{
    public sealed partial class EFEntityTypeSortingKeyTopology : EFEntityTypeSortingKeyTopology.ILastProperty
    {
        internal const bool NeverDescendingToAvoidTopologyCachesBloating = false;

        public ICollationAwareModelProvider ModelProvider { get; }
        public IEntityType EntityType { get; }
        public IProperty Property { get; }
        SortingKeyTopology.ILastProperty SortingKeyTopology.ILastProperty.Prefix => Prefix;
        ILastProperty ILastProperty.Prefix => Prefix;
        internal EFEntityTypeSortingKeyTopology Prefix { get; }
        public int PrefixKeysCount { get; }
        bool SortingKeyTopology.ILastProperty.MultiplePropertyOwners => false;
        bool SortingKeyTopology.ILastProperty.UniquePropertyNames => true;
        public SortingKeyTopology.PropertiesOwner PropertyOwner { get; }
        public string PropertyName { get; }
        public Type PropertyValueType { get; }
        bool SortingKeyTopology.ILastProperty.Descending => NeverDescendingToAvoidTopologyCachesBloating;

        internal EFEntityTypeSortingKeyTopology(ICollationAwareModelProvider modelProvider, IEntityType entityType)
        {
            ModelProvider = modelProvider ?? throw new ArgumentNullException(nameof(modelProvider));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            EFEntityTypeSortingKeyTopology prefix = null;

            var properties = entityType.GetProperties()?.ToList() ?? throw new ApplicationException("properties == null");
            if (properties.Count == 0) throw new ApplicationException("properties.Count == 0");
            int lastIndex = properties.Count - 1;
            for (int i = 0; i < lastIndex; i++)
                prefix = new EFEntityTypeSortingKeyTopology(ModelProvider, entityType, properties[i], prefix);

            var lastProperty = properties[lastIndex];
            Property = lastProperty;
            Prefix = prefix;
            PrefixKeysCount = prefix?.PrefixKeysCount + 1 ?? 0;
            PropertyOwner = new SortingKeyTopology.PropertiesOwner
            {
                EntityType = lastProperty.DeclaringType?.ClrType ?? throw new ApplicationException("lastProperty.DeclaringType?.ClrType == null"),
                EntityName = entityType.HasSharedClrType ? lastProperty.DeclaringType.Name : null
            };
            PropertyName = lastProperty.Name ?? throw new ApplicationException("lastProperty.Name == null");
            PropertyValueType = lastProperty.ClrType ?? throw new ApplicationException("lastProperty.ClrType == null");
        }

        private EFEntityTypeSortingKeyTopology(
            ICollationAwareModelProvider modelProvider,
            IEntityType entityType,
            IProperty property,
            EFEntityTypeSortingKeyTopology prefix)
        {
            ModelProvider = modelProvider ?? throw new ArgumentNullException(nameof(modelProvider));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Prefix = prefix;
            PrefixKeysCount = prefix?.PrefixKeysCount + 1 ?? 0;
            PropertyOwner = new SortingKeyTopology.PropertiesOwner
            {
                EntityType = property.DeclaringType?.ClrType ?? throw new ApplicationException("lastProperty.DeclaringType?.ClrType == null"),
                EntityName = property.DeclaringType.HasSharedClrType ? property.DeclaringType.Name : null
            };
            PropertyName = property.Name ?? throw new ApplicationException("property.Name == null");
            PropertyValueType = property.ClrType ?? throw new ApplicationException("property.ClrType == null");
        }

        public interface ILastProperty : SortingKeyTopology.ILastProperty
        {
            IProperty Property { get; }
            new ILastProperty Prefix { get; }
        }
    }
}