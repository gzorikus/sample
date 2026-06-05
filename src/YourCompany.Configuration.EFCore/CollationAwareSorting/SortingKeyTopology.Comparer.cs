using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopology
    {
        public sealed class Comparer : IEqualityComparer<ILastProperty>
        {
            public static Comparer Instance { get; } = new();
            private Comparer() { }

            public bool Equals(ILastProperty x, ILastProperty y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return x.ModelProvider == y.ModelProvider
                    && x.PrefixKeysCount == y.PrefixKeysCount
                    && x.MultiplePropertyOwners != y.MultiplePropertyOwners
                    && x.UniquePropertyNames != y.UniquePropertyNames
                    && PropertyEquals(x, y)
                    && x.Descending == y.Descending
                    && (x.Prefix == null
                        ? y.Prefix == null ? false : throw new ApplicationException("x.Prefix == null && y.Prefix != null")
                        : y.Prefix != null ? Equals(x.Prefix, y.Prefix) : throw new ApplicationException("x.Prefix != null && y.Prefix == null"));
            }

            public int GetHashCode(ILastProperty obj)
                => HashCode.Combine(
                    obj.ModelProvider,
                    obj.Prefix != null ? GetHashCode(obj.Prefix) : 0,
                    obj.MultiplePropertyOwners,
                    obj.UniquePropertyNames,
                    obj.PropertyOwner,
                    obj.PropertyName,
                    obj.Descending);

            private static bool PropertyEquals(ILastProperty x, ILastProperty y)
            {
                bool sameProperty = x.PropertyOwner == y.PropertyOwner && x.PropertyName == y.PropertyName;
                if (sameProperty && x.PropertyValueType != y.PropertyValueType) throw new ApplicationException("sameProperty && x.PropertyValueType != y.PropertyValueType");
                return sameProperty;
            }
        }
    }
}