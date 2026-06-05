using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey
    {
        public partial class ValueHolding<TValue> : SortingKey where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            private static readonly Func<TValue, TValue> NoConversion = v => v;

            private Func<TValue, TValue> _conversion;
            private IEqualityComparer<TValue> _equalityComparer;
            private IComparer<TValue> _comparer;
            private TValue _convertedValue;
            private bool _convertedValueSet;

            public override Type PropertyValueType => typeof(TValue);
            public TValue Value { get; private set; }

            public Func<TValue, TValue> Conversion => _conversion
                ??= ModelProvider.GetProviderTypeConversionFromValueConverterIfSet<TValue, TValue>(
                        PropertyOwner, PropertyName)
                    ?? NoConversion;

            public IEqualityComparer<TValue> EqualityComparer => _equalityComparer
                ??= ModelProvider.GetModelEqualityComparerRequired<TValue>(PropertyOwner, PropertyName);

            public IComparer<TValue> Comparer => _comparer
                ??= ModelProvider.GetCollationAwareStringComparerOrDefault<TValue>(PropertyOwner, PropertyName);

            internal ValueHolding(ICollationAwareModelProvider modelProvider, SortingKey prefixKey)
                : base(modelProvider, prefixKey)
            {
                if (PropertyValueType == typeof(TValue) && GetType() != typeof(ValueHolding<TValue>))
                    throw new ApplicationException("PropertyValueType == typeof(TValue) && GetType() != typeof(ValueHolding<TValue>)");
            }

            internal void SetValue(
                SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, TValue value, bool descending)
            {
                if (PropertyValueType != typeof(TValue)) throw new ApplicationException("PropertyValueType != typeof(TValue)");
                SetProperty(propertyOwner, propertyName, descending);
                Value = value;
            }

            public override int VisitPrefixFirst(SortingKeyTopology.IPrefixFirstPropertiesVisitor visitor)
            {
                if (visitor == null) throw new ArgumentNullException(nameof(visitor));
                if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                visitor.ModelProvider ??= ModelProvider;
                if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
                int count = PrefixKey?.VisitPrefixFirst(visitor) ?? 0;
                visitor.VisitProperty<TValue>(this);
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

            protected override SortingKey Clone(bool descending, SortingKey clonedPrefix)
            {
                var clone = new ValueHolding<TValue>(ModelProvider, clonedPrefix);
                clone.SetValue(PropertyOwner, PropertyName, Value, descending);
                return clone;
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
                        ? queryable.OrderByDescending(obj => EF.Property<TValue>(obj, PropertyName))
                        : queryable.OrderBy(obj => EF.Property<TValue>(obj, PropertyName));
                }
                else
                {
                    var orderedQueryable = PrefixKey.SingleEntitySortRecursive(queryable, replacingQueriedSet);
                    return Descending
                        ? orderedQueryable.ThenByDescending(obj => EF.Property<TValue>(obj, PropertyName))
                        : orderedQueryable.ThenBy(obj => EF.Property<TValue>(obj, PropertyName));
                }
            }

            protected override bool CheckMatchingTopologyValuesEqualRecursive(SortingKey other)
            {
                if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                if (other is not ValueHolding<TValue> otherSortingKey) throw new ApplicationException("other is not ValueHolding<TValue> otherSortingKey");
                bool prefixValuesEqual = PrefixKey == null
                    || PrefixKey.CheckMatchingTopologyValuesEqualRecursive(other.PrefixKey);
                return prefixValuesEqual && EqualityComparer.Equals(Value, otherSortingKey.Value);
            }

            protected override int GetValuesHashCodeRecursive()
            {
                if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                var hash = new HashCode();
                if (PrefixKey != null) hash.Add(PrefixKey.GetValuesHashCodeRecursive());
                if (Value is not null) hash.Add(EqualityComparer.GetHashCode(Value));
                return hash.ToHashCode();
            }

            protected override int CompareMatchingTopologyValuesRecursivePrefixFirst(SortingKey other)
            {
                if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                if (other is not ValueHolding<TValue> otherSortingKey) throw new ApplicationException("other is not ValueHolding<TValue> otherSortingKey");
                int comparePrefix = PrefixKey?.CompareMatchingTopologyValuesRecursivePrefixFirst(other.PrefixKey) ?? 0;
                return comparePrefix != 0
                    ? comparePrefix
                    : Conversion == null
                        ? Comparer.Compare(Value, otherSortingKey.Value)
                        : Comparer.Compare(GetConvertedValue(), otherSortingKey.GetConvertedValue());
            }

            protected override string ToStringValue()
            {
                string value = Value is null ? "NULL" : Value.ToString();
                if (Conversion != NoConversion) value = $"{GetConvertedValue()}←{value}";
                return value;
            }

            private TValue GetConvertedValue()
            {
                if (PropertyName == null) throw new ApplicationException("PropertyName == null");
                if (Conversion == NoConversion) throw new ApplicationException("Conversion == NoConversion");
                if (PropertyValueType != typeof(TValue)) throw new ApplicationException("PropertyValueType != typeof(TValue)");
                if (!_convertedValueSet)
                {
                    var converstion = _conversion ?? throw new ApplicationException("_conversion == null");
                    _convertedValue = converstion(Value);
                    _convertedValueSet = true;
                }
                return _convertedValue;
            }
        }
    }
}