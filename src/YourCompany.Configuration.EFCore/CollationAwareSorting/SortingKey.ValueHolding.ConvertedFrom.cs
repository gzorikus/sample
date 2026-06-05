using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey
    {
        public partial class ValueHolding<TValue>
        {
            public sealed class ConvertedFrom<TModelValue> : ValueHolding<TValue>
            {
                private static readonly IEqualityComparer<TModelValue> NoEqualityComparer = new NotImplementedComparer();

                private new Func<TModelValue, TValue> _conversion;
                private IEqualityComparer<TModelValue> _modelEqualityComparer;

                public override Type PropertyValueType => typeof(TModelValue);
                public new TModelValue Value { get; private set; }

                public new Func<TModelValue, TValue> Conversion => _conversion
                    ??= ModelProvider.GetProviderTypeConversionFromValueConverterIfSet<TModelValue, TValue>(
                            PropertyOwner, PropertyName)
                        ?? throw new ApplicationException("conversion == null");

                public IEqualityComparer<TModelValue> ModelEqualityComparer => _modelEqualityComparer
                    ??= ModelProvider.GetModelEqualityComparerIfAvailable<TModelValue>(PropertyOwner, PropertyName)
                        ?? NoEqualityComparer;

                internal ConvertedFrom(ICollationAwareModelProvider modelProvider, SortingKey prefixKey)
                    : base(modelProvider, prefixKey) { }

                internal void SetValue(
                    SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, TModelValue value, bool descending)
                {
                    if (PropertyValueType != typeof(TModelValue)) throw new ApplicationException("PropertyValueType != typeof(TModelValue)");
                    SetProperty(propertyOwner, propertyName, descending);
                    Value = value;
                }

                protected override SortingKey Clone(bool descending, SortingKey clonedPrefix)
                {
                    var clone = new ConvertedFrom<TModelValue>(ModelProvider, clonedPrefix);
                    clone.SetValue(PropertyOwner, PropertyName, Value, descending);
                    return clone;
                }

                public override int VisitPrefixFirst(SortingKeyTopology.IPrefixFirstPropertiesVisitor visitor)
                {
                    if (visitor == null) throw new ArgumentNullException(nameof(visitor));
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    visitor.ModelProvider ??= ModelProvider;
                    if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
                    int count = PrefixKey?.VisitPrefixFirst(visitor) ?? 0;
                    visitor.VisitProperty<TValue, TModelValue>(this);
                    return count + 1;
                }

                public override int VisitPrefixFirst(IPrefixFirstPropertiesVisitor visitor)
                {
                    if (visitor == null) throw new ArgumentNullException(nameof(visitor));
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    visitor.ModelProvider ??= ModelProvider;
                    if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
                    int count = PrefixKey?.VisitPrefixFirst(visitor) ?? 0;
                    visitor.VisitProperty(this);
                    return count + 1;
                }

                protected override IOrderedQueryable<TEntity> SingleEntitySortRecursive<TEntity>(
                    IQueryable<TEntity> queryable, bool replacingQueriedSet)
                    where TEntity : class
                {
                    if (queryable == null) throw new ArgumentNullException(nameof(queryable));
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");

                    this.EnsureCompatibleWithSingleEntityQueries(
                        typeof(TEntity), singleEntityReplacingQueriedSet: replacingQueriedSet);

                    if (PrefixKey == null)
                    {
                        return Descending
                            ? queryable.OrderByDescending(obj => EF.Property<TModelValue>(obj, PropertyName))
                            : queryable.OrderBy(obj => EF.Property<TModelValue>(obj, PropertyName));
                    }
                    else
                    {
                        var orderedQueryable = PrefixKey.SingleEntitySortRecursive(queryable, replacingQueriedSet);
                        return Descending
                            ? orderedQueryable.ThenByDescending(obj => EF.Property<TModelValue>(obj, PropertyName))
                            : orderedQueryable.ThenBy(obj => EF.Property<TModelValue>(obj, PropertyName));
                    }
                }

                protected override bool CheckMatchingTopologyValuesEqualRecursive(SortingKey other)
                {
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    if (other is not ConvertedFrom<TModelValue> otherSortingKey) throw new ApplicationException("other is not ConvertedFrom<TModelValue> otherSortingKey");
                    bool prefixValuesEqual = PrefixKey == null || PrefixKey.CheckMatchingTopologyValuesEqualRecursive(other.PrefixKey);
                    return prefixValuesEqual
                        && ModelEqualityComparer != NoEqualityComparer
                            ? _modelEqualityComparer?.Equals(Value, otherSortingKey.Value) ?? throw new ApplicationException("_modelEqualityComparer == null")
                            : EqualityComparer.Equals(GetConvertedValue(), otherSortingKey.GetConvertedValue());
                }

                protected override int GetValuesHashCodeRecursive()
                {
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    var hash = new HashCode();
                    if (PrefixKey != null) hash.Add(PrefixKey.GetValuesHashCodeRecursive());

                    if (ModelEqualityComparer != NoEqualityComparer)
                    {
                        if (Value is not null) hash.Add(ModelEqualityComparer.GetHashCode(Value));
                    }
                    else
                    {
                        if (GetConvertedValue() is not null) hash.Add(EqualityComparer.GetHashCode(GetConvertedValue()));
                    }

                    return hash.ToHashCode();
                }

                protected override int CompareMatchingTopologyValuesRecursivePrefixFirst(SortingKey other)
                {
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    if (other is not ConvertedFrom<TValue> otherSortingKey) throw new ApplicationException("other is not ConvertedFrom<TValue> otherSortingKey");
                    int comparePrefix = PrefixKey?.CompareMatchingTopologyValuesRecursivePrefixFirst(other.PrefixKey) ?? 0;
                    return comparePrefix != 0
                        ? comparePrefix
                        : Comparer.Compare(GetConvertedValue(), otherSortingKey.GetConvertedValue());
                }

                protected override string ToStringValue()
                    => $"{(GetConvertedValue() is null ? "NULL" : GetConvertedValue())}←{(Value is null ? "NULL" : Value.ToString())}";

                private new TValue GetConvertedValue()
                {
                    if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                    if (Conversion == null) throw new ApplicationException("Conversion == null");
                    if (PropertyValueType != typeof(TModelValue)) throw new ApplicationException("PropertyValueType != typeof(TModelValue)");
                    if (!_convertedValueSet)
                    {
                        var converstion = _conversion ?? throw new ApplicationException("_conversion == null");
                        _convertedValue = converstion(Value);
                        _convertedValueSet = true;
                    }
                    return _convertedValue;
                }

                private class NotImplementedComparer : IEqualityComparer<TModelValue>
                {
                    public bool Equals(TModelValue x, TModelValue y) => throw new NotImplementedException(nameof(Equals));
                    public int GetHashCode(TModelValue obj) => throw new NotImplementedException(nameof(GetHashCode));
                }
            }
        }
    }
}