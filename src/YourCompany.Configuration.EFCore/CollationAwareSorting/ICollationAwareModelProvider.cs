using System;
using System.Collections.Generic;
using YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public interface ICollationAwareModelProvider
    {
        string GetEntityTableName(SortingKeyTopology.PropertiesOwner propertiesOwner);
        EFEntityTypeSortingKeyTopology.ILastProperty GetEntityTypeTopology(SortingKeyTopology.PropertiesOwner propertiesOwner);
        Type GetPropertyValueType(SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName);
        Type GetPropertyProviderValueTypeIfDiffers(SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName);

        Func<TModel, TProvider> GetProviderTypeConversionFromValueConverterIfSet<TModel, TProvider>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
            where TProvider : IEquatable<TProvider>, IComparable<TProvider>;

        IEqualityComparer<T> GetCollationAwareEqualityComparerExceptStringFromValueComparerIfSet<T>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName);

        IEqualityComparer<string> GetCollationCompatibleStringEqualityComparer(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName);

        IComparer<string> GetCollationCompatibleStringComparer(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName);
    }

    public static class CollationAwareModelProviderExtensions
    {
        public static SortingKeyBuilder<TValue> GetKeyForSortingWithProperty<TValue>(
            this ICollationAwareModelProvider modelProvider,
            Type entityType,
            string name,
            TValue value,
            bool descending = false)
        {
            var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
            return new(modelProvider, owner, name, value, descending);
        }

        public static SortingKeyBuilder<TValue> GetKeyForSortingWithProperty<TValue>(
            this ICollationAwareModelProvider modelProvider,
            string entityName,
            string name,
            TValue value,
            bool descending = false,
            Type matchSharedEntityType = null)
        {
            var owner = new SortingKeyTopology.PropertiesOwner
            {
                EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                EntityType = matchSharedEntityType
            };
            return new(modelProvider, owner, name, value, descending);
        }

        public static bool CheckPropertyValuesEqual<T>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            T x,
            T y)
            where T : IEquatable<T>, IComparable<T>
            => modelProvider.GetModelEqualityComparerRequired<T>(propertyOwner, propertyName).Equals(x, y);

        public static bool CheckPropertyValuesEqual<TModel, TProvider>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            TModel x,
            TModel y)
            where TProvider : IEquatable<TProvider>, IComparable<TProvider>
        {
            var equalityComparer = modelProvider.GetModelEqualityComparerIfAvailable<TModel>(propertyOwner, propertyName);
            var conversion = modelProvider
                .GetProviderTypeConversionFromValueConverterIfSet<TModel, TProvider>(propertyOwner, propertyName)
                    ?? throw new ApplicationException("conversion == null");
            return equalityComparer?.Equals(x, y)
                ?? modelProvider.GetCollationAwareStringEqualityComparerOrDefault<TProvider>(propertyOwner, propertyName)
                    .Equals(conversion(x), conversion(y));
        }

        public static IEqualityComparer<TProperty> GetModelEqualityComparerRequired<TProperty>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName)
            => modelProvider.GetModelEqualityComparerIfAvailable<TProperty>(propertyOwner, propertyName)
                ?? throw new ApplicationException("equalityComparer == null");

        public static IEqualityComparer<TProperty> GetModelEqualityComparerIfAvailable<TProperty>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            return modelProvider
                .GetCollationAwareEqualityComparerExceptStringFromValueComparerIfSet<TProperty>(propertyOwner, propertyName)
                    ?? modelProvider
                        .GetCollationAwareStringEqualityComparerOrDefaultIfEquatable<TProperty>(propertyOwner, propertyName);
        }

        public static IEqualityComparer<T> GetCollationAwareStringEqualityComparerOrDefault<T>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName)
            where T : IEquatable<T>
            => modelProvider.GetCollationAwareStringEqualityComparerOrDefaultIfEquatable<T>(propertyOwner, propertyName)
                ?? throw new ApplicationException("equalityComparer == null");

        public static IEqualityComparer<T> GetCollationAwareStringEqualityComparerOrDefaultIfEquatable<T>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (!typeof(IEquatable<T>).IsAssignableFrom(typeof(T))) return null;
            return typeof(T) == typeof(string)
                ? (IEqualityComparer<T>)modelProvider
                    .GetCollationCompatibleStringEqualityComparer(propertyOwner, propertyName)
                        ?? throw new ApplicationException("equalityComparer == null")
                : EqualityComparer<T>.Default;
        }

        public static IComparer<T> GetCollationAwareStringComparerOrDefault<T>(
            this ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName)
            where T : IEquatable<T>, IComparable<T>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            return typeof(T) == typeof(string)
                ? (IComparer<T>)modelProvider
                    .GetCollationCompatibleStringComparer(propertyOwner, propertyName)
                        ?? throw new ApplicationException("comparer == null")
                : Comparer<T>.Default;
        }
    }
}