using System;
using System.Collections;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.Reflection.Composition;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection
{
    public sealed class RecordTypesCompositionMap : IReadOnlyDictionary<Type, ComposableRecordTypeInfo>
    {
        private readonly Dictionary<Type, ComposableRecordTypeInfo> _types;

        public TypesCompositionMap TypesCompositionMap { get; }
        public RecordTypesMap RecordTypesMap { get; }
        public IEnumerable<Type> Keys => _types.Keys;
        public IEnumerable<ComposableRecordTypeInfo> Values => _types.Values;
        public int Count => _types.Count;
        public ComposableRecordTypeInfo this[Type key] => _types[key];

        public RecordTypesCompositionMap(IEnumerable<ComposableRecordTypesProvider> domainTypesProviders)
        {
            _types = new Dictionary<Type, ComposableRecordTypeInfo>();
            TypesCompositionMap = new TypesCompositionMap(domainTypesProviders);
            RecordTypesMap = new RecordTypesMap(_types.Keys, GetExtraRecordConstructionLimitationReason);
            Populate();
        }

        public bool ContainsKey(Type key) => _types.ContainsKey(key);
        public bool TryGetValue(Type key, out ComposableRecordTypeInfo value) => _types.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<Type, ComposableRecordTypeInfo>> GetEnumerator() => _types.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();

        private void Populate()
        {
            foreach (var composableTypeInfo in TypesCompositionMap.Values)
                _types.Add(composableTypeInfo.Type,
                    new ComposableRecordTypeInfo(this, RecordTypesMap[composableTypeInfo.Type], composableTypeInfo, _types));
        }

        private RecordConstructionException GetExtraRecordConstructionLimitationReason(RecordTypeInfo recordTypeInfo)
        {
            var composableTypeInfo = TypesCompositionMap[recordTypeInfo.Type];
            return composableTypeInfo.CheckComposableHasConstructionLimitationReason(typeof(CompositionException))
                ? new RecordCompositionToConstructionWrapperException(
                    composableTypeInfo.GetCompositionExceptionForThrowing, recordTypeInfo.ConstructorInfo)
                : null;
        }
    }
}