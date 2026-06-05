using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata
{
    internal sealed class CollationAwareSortingSingleModelProvider : ICollationAwareModelProvider
    {
        private readonly IModel _model;
        private readonly ICollationCompatibleComparersProvider _comparersProvider;

        internal CollationAwareSortingSingleModelProvider(
            IModel model, ICollationCompatibleComparersProvider comparersProvider)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _comparersProvider = comparersProvider ?? throw new ArgumentNullException(nameof(comparersProvider));
        }

        public string GetEntityTableName(SortingKeyTopology.PropertiesOwner propertiesOwner)
        {
            if (propertiesOwner == default) throw new ArgumentNullException(nameof(propertiesOwner));
            var entityTypeInfo = _model.FindEntityType(propertiesOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            return entityTypeInfo.GetTableName() ?? throw new ApplicationException("tableName == null");
        }

        public Type GetPropertyValueType(SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            return property.ClrType;
        }

        public Type GetPropertyProviderValueTypeIfDiffers(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            var providerClrType = property.GetProviderClrType();
            return property.ClrType != providerClrType ? providerClrType : null;
        }

        public Func<TModel, TProvider> GetProviderTypeConversionFromValueConverterIfSet<TModel, TProvider>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
            where TProvider : IEquatable<TProvider>, IComparable<TProvider>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            var valueConverter = property.GetValueConverter();
            return valueConverter == null
                ? null
                : valueConverter is ValueConverter<TModel, TProvider> typed
                    ? ValueConverterStrategiesCache<TModel, TProvider>.GetConvertToProviderTyped(typed)
                    : throw new ApplicationException("valueConverter != null && valueConverter is not ValueConverter<TModel, TProvider> typed");
        }

        public IEqualityComparer<T> GetCollationAwareEqualityComparerExceptStringFromValueComparerIfSet<T>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (typeof(T) == typeof(string)) return null;
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            var valueComparer = property.GetValueComparer();
            return valueComparer == null
                ? null
                : valueComparer as IEqualityComparer<T> ?? throw new ApplicationException("valueComparer is not IEqualityComparer<T>");
        }

        public IEqualityComparer<string> GetCollationCompatibleStringEqualityComparer(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            if (property.ClrType != typeof(string) && property.GetProviderClrType() != typeof(string))
                throw new ApplicationException("property.ClrType != typeof(string) && property.GetProviderClrType() != typeof(string)");
            return _comparersProvider.GetCollationCompatibleStringEqualityComparer(
                collationNameOrProviderDefault: property.GetCollation());
        }

        public IComparer<string> GetCollationCompatibleStringComparer(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var entityTypeInfo = _model.FindEntityType(propertyOwner) ?? throw new ApplicationException("entityTypeInfo == null");
            var property = entityTypeInfo.FindProperty(propertyName) ?? throw new ApplicationException("property == null");
            if (property.ClrType != typeof(string) && property.GetProviderClrType() != typeof(string))
                throw new ApplicationException("property.ClrType != typeof(string) && property.GetProviderClrType() != typeof(string)");
            return _comparersProvider.GetCollationCompatibleStringComparer(
                collationNameOrProviderDefault: property.GetCollation());
        }

        private static class ValueConverterStrategiesCache<TModel, TProvider>
        {
#if NET8_0_OR_GREATER
            internal static Func<TModel, TProvider> GetConvertToProviderTyped(ValueConverter<TModel, TProvider> converter)
                => converter.ConvertToProviderTyped;
#else
            private static readonly ConcurrentDictionary<ValueConverter<TModel, TProvider>, Func<TModel, TProvider>>
                ConvertToProviderTyped = new();

            private static readonly Func<ValueConverter<TModel, TProvider>, Func<TModel, TProvider>>
                ConvertToProviderTypedCreateItemNonExclusiveStrategy
                    = converter => converter.ConvertToProviderExpression.Compile();

            internal static Func<TModel, TProvider> GetConvertToProviderTyped(ValueConverter<TModel, TProvider> converter)
                => ConvertToProviderTyped.GetOrAdd(converter, ConvertToProviderTypedCreateItemNonExclusiveStrategy);
#endif
        }
    }
}