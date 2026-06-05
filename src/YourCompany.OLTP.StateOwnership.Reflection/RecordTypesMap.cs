using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCompany.OLTP.StateOwnership.Reflection
{
    public sealed class RecordTypesMap : IReadOnlyDictionary<Type, RecordTypeInfo>
    {
        private readonly Dictionary<Type, RecordTypeInfo> _types;
        private readonly Func<RecordTypeInfo, RecordConstructionException> _getExtraRecordConstructionLimitationReason;

        public IEnumerable<Type> Keys => _types.Keys;
        public IEnumerable<RecordTypeInfo> Values => _types.Values;
        public int Count => _types.Count;
        public RecordTypeInfo this[Type key] => _types[key];

        public RecordTypesMap(IEnumerable<Type> typesToMap, Func<RecordTypeInfo, RecordConstructionException> getExtraRecordConstructionLimitationReason = null)
        {
            _types = new Dictionary<Type, RecordTypeInfo>();
            Populate(typesToMap);
            _getExtraRecordConstructionLimitationReason = getExtraRecordConstructionLimitationReason;
        }

        public bool ContainsKey(Type key) => _types.ContainsKey(key);
        public bool TryGetValue(Type key, out RecordTypeInfo value) => _types.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<Type, RecordTypeInfo>> GetEnumerator() => _types.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();

        private void Populate(IEnumerable<Type> typesToMap)
        {
            if (typesToMap == null) throw new ArgumentNullException(nameof(typesToMap));
            foreach (var possiblyRecordType in typesToMap)
                _ = new RecordTypeInfo(this, possiblyRecordType, _types, _getExtraRecordConstructionLimitationReason);
        }
    }
}