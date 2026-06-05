using System;
using System.Collections;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.Reflection.Composition;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection
{
    public sealed class ComposableRecordTypeInfo : IReadOnlyDictionary<Type, ComposableRecordTypeInfo>
    {
        private readonly IDictionary<Type, ComposableRecordTypeInfo> _types;

        public RecordTypesCompositionMap Map { get; }
        public RecordTypeInfo RecordTypeInfo { get; }
        public ComposableTypeInfo ComposableTypeInfo { get; }
        public Type Type => RecordTypeInfo.Type;
        public Type RecordDataType => RecordTypeInfo.RecordDataType;
        public bool IsRecord => RecordTypeInfo.IsRecord;
        public bool IsEntity => IsRecord && !IsMixin;
        public bool IsMixin => IsRecord && ComposableTypeInfo.IsMixin;
        public bool IsCrossCuttingMixin => IsRecord && ComposableTypeInfo.IsCrossCuttingMixin;
        public IEnumerable<Type> Keys => ComposableTypeInfo.Keys;

        public IEnumerable<ComposableRecordTypeInfo> Values
        {
            get
            {
                foreach (var dependencyComposableTypeInfo in ComposableTypeInfo.Values)
                    yield return _types[dependencyComposableTypeInfo.Type];
            }
        }

        public int Count => ComposableTypeInfo.Count;
        public ComposableRecordTypeInfo this[Type key] => _types[ComposableTypeInfo[key].Type];

        internal ComposableRecordTypeInfo(
            RecordTypesCompositionMap map,
            RecordTypeInfo recordTypeInfo,
            ComposableTypeInfo composableTypeInfo,
            IDictionary<Type, ComposableRecordTypeInfo> types)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            RecordTypeInfo = recordTypeInfo ?? throw new ArgumentNullException(nameof(recordTypeInfo));
            ComposableTypeInfo = composableTypeInfo ?? throw new ArgumentNullException(nameof(composableTypeInfo));
            if (recordTypeInfo.Type != composableTypeInfo.Type) throw new ApplicationException("recordTypeInfo.Type != composableTypeInfo.Type");
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _types.Add(composableTypeInfo.Type, this);
        }

        public bool ContainsKey(Type key) => ComposableTypeInfo.ContainsKey(key);
        public bool TryGetValue(Type key, out ComposableRecordTypeInfo value) => _types.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<Type, ComposableRecordTypeInfo>> GetEnumerator()
        {
            foreach (var dependencyComposableTypeInfo in ComposableTypeInfo.Values)
                yield return new KeyValuePair<Type, ComposableRecordTypeInfo>(
                    dependencyComposableTypeInfo.Type, _types[dependencyComposableTypeInfo.Type]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}