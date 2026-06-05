using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public readonly partial struct SortingKeyBuilder<TValue>
    {
        private readonly ICollationAwareModelProvider _modelProvider;
        private readonly SortingKeyTopology.PropertiesOwner _owner;
        private readonly string _name;
        private readonly TValue _value;
        private readonly bool _descending;

        internal SortingKey PrefixKey { get; init; }

        internal SortingKeyBuilder(
            ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner owner,
            string name,
            TValue value,
            bool descending)
        {
            if (owner == default) throw new ApplicationException("owner == default");
            _modelProvider = modelProvider ?? throw new ArgumentNullException(nameof(modelProvider));
            _owner = owner;
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _value = value;
            _descending = descending;
            PrefixKey = null;
        }

        public ComparableValue<TProviderValue> ComparedBy<TProviderValue>()
            where TProviderValue : IEquatable<TProviderValue>, IComparable<TProviderValue>
            => new() { KeyBuilder = this };
    }
}