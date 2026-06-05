using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopology
    {
        public readonly record struct PropertiesOwner
        {
            private readonly Type _entityType;

            public Type EntityType
            {
                get => _entityType;
                init => _entityType = value == null || !value.IsValueType ? value : throw new ApplicationException("value != null && value.IsValueType");
            }

            public string EntityName { get; init; }
            public bool IsSharedEntityType => EntityName != null;

            public static PropertiesOwner From(IEntityType entityType) => new()
            {
                EntityType = entityType.ClrType ?? throw new ApplicationException("entityType.ClrType == null"),
                EntityName = entityType.HasSharedClrType ? entityType.Name : null
            };

            public bool MatchSingleTopology(PropertiesOwner other)
            {
                if (this == default) throw new ApplicationException("this == default");
                if (other == default) throw new ApplicationException("other == default");

                if (IsSharedEntityType != other.IsSharedEntityType) return false;

                bool entityTypeEquals = EntityType == other.EntityType;
                bool entityNameEquals = EntityName == other.EntityName;

                if (IsSharedEntityType && entityNameEquals && !entityTypeEquals)
                    throw new ApplicationException("IsSharedEntityType && entityNameEquals && !entityTypeEquals");

                bool matched = IsSharedEntityType ? entityNameEquals : entityTypeEquals;
                bool equals = this == other;
                if (matched != equals) throw new ApplicationException("matched != equals");
                return matched;
            }

            public bool Match(IReadOnlyTypeBase entityType)
            {
                if (entityType == null) throw new ArgumentNullException(nameof(entityType));
                if (this == default) throw new ApplicationException("this == default");

                if (IsSharedEntityType != entityType.HasSharedClrType) return false;

                var entityClrType = entityType.ClrType ?? throw new ApplicationException("entityType.ClrType == null");
                bool entityTypePass = Match(entityClrType);
                bool entityNamePass = EntityName == null || Match(entityType.Name);

                if (EntityType != null && EntityName != null && entityNamePass && !entityTypePass)
                    throw new ApplicationException("EntityType != null && EntityName != null && entityNamePass && !entityTypePass");

                return entityTypePass && entityNamePass;
            }

            public bool Match(Type entityType)
            {
                if (entityType == null) throw new ArgumentNullException(nameof(entityType));
                if (this == default) throw new ApplicationException("this == default");
                return MatchUnique(entityType) || MatchShared(entityType);
            }

            public bool MatchUnique(Type entityType)
            {
                if (entityType == null) throw new ArgumentNullException(nameof(entityType));
                if (this == default) throw new ApplicationException("this == default");
                return !IsSharedEntityType && EntityType.IsAssignableFrom(entityType);
            }

            public bool MatchShared(Type entityType)
            {
                if (entityType == null) throw new ArgumentNullException(nameof(entityType));
                if (this == default) throw new ApplicationException("this == default");
                if (!IsSharedEntityType) return false;
                return EntityType == null || EntityType == entityType;
            }

            public bool Match(string sharedEntityTypeEntityName)
            {
                if (sharedEntityTypeEntityName == null) throw new ArgumentNullException(nameof(sharedEntityTypeEntityName));
                if (this == default) throw new ApplicationException("this == default");
                return IsSharedEntityType && EntityName == sharedEntityTypeEntityName;
            }

            public override string ToString() => IsSharedEntityType ? EntityName : EntityType?.FullName ?? "N/A";
        }
    }
}