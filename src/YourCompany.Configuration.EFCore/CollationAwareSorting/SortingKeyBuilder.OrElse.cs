using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public readonly partial struct SortingKeyBuilder<TValue>
    {
        public readonly partial struct OrElse
        {
            private readonly OrElseEqualityKeys _keys;

            internal SortingKeyBuilder<TValue> KeyBuilder { get; init; }

            internal OrElse(SortingKey keyEquality)
            {
                if (keyEquality == null) throw new ArgumentNullException(nameof(keyEquality));
                _keys = new OrElseEqualityKeys();
                _keys.AddAndTryRetainSingleTopology(keyEquality);
                KeyBuilder = default;
            }

            internal OrElse(OrElseEqualityKeys keys)
            {
                _keys = keys ?? throw new ArgumentNullException(nameof(keys));
                KeyBuilder = default;
            }

            public ComparableValue<TProviderValue> ComparedBy<TProviderValue>()
                where TProviderValue : IEquatable<TProviderValue>, IComparable<TProviderValue>
                => new() { OrElse = this, KeyBuilder = KeyBuilder.ComparedBy<TProviderValue>() };
        }
    }
}