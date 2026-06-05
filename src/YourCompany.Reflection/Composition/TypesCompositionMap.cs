using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed class TypesCompositionMap : IReadOnlyDictionary<Type, ComposableTypeInfo>
    {
        private readonly Dictionary<Type, ComposableTypeInfo> _types;

        public IEnumerable<Type> Keys => _types.Keys;
        public IEnumerable<ComposableTypeInfo> Values => _types.Values;
        public int Count => _types.Count;
        public ComposableTypeInfo this[Type key] => _types[key];

        public TypesCompositionMap(IEnumerable<ComposableTypesProvider> composableTypesProviders)
        {
            _types = new Dictionary<Type, ComposableTypeInfo>();
            Populate(composableTypesProviders);
        }

        public bool ContainsKey(Type key) => _types.ContainsKey(key);
        public bool TryGetValue(Type key, out ComposableTypeInfo value) => _types.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<Type, ComposableTypeInfo>> GetEnumerator() => _types.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();

        private void Populate(IEnumerable<ComposableTypesProvider> composableTypesProviders)
        {
            if (composableTypesProviders == null)
                throw new ArgumentNullException(nameof(composableTypesProviders));

            foreach (var composableTypesProvider in composableTypesProviders)
                foreach (var possiblyComposableType in composableTypesProvider.GetTypesToMap())
                    _ = new ComposableTypeInfo(this, composableTypesProvider, possiblyComposableType, _types);

            foreach (var composableTypeInfo in _types.Values)
            {
                if (!composableTypeInfo.IsComposable) continue;
                composableTypeInfo.CollectComposableConstructorDependenciesAndRegisterAbstractionsImplementation();
            }

            foreach (var possibleMixinTypeInfo in _types.Values)
            {
                if (!possibleMixinTypeInfo.IsComposable || possibleMixinTypeInfo.IsCrossCuttingMixin) continue;
                possibleMixinTypeInfo.TryDetermineComposableAsMixin();
            }

            foreach (var crossCuttingMixinTypeInfo in _types.Values)
            {
                if (!crossCuttingMixinTypeInfo.IsCrossCuttingMixin) continue;
                foreach (var composableTypeInfo in _types.Values)
                {
                    if (!composableTypeInfo.IsComposable || composableTypeInfo.IsMixin) continue;
                    if (crossCuttingMixinTypeInfo.CheckRootWasCollectedByCrossCuttingMixin(composableTypeInfo)) break;
                    crossCuttingMixinTypeInfo.CollectCrossCuttingMixinApplicableRoots(composableTypeInfo);
                }
            }

            foreach (var determinedMixinTypeInfo in _types.Values)
            {
                if (!determinedMixinTypeInfo.IsMixin || determinedMixinTypeInfo.IsCrossCuttingMixin) continue;
                determinedMixinTypeInfo.CollectDeterminedMixinsApplicableRoots();
            }

            foreach (var composableTypeInfo in _types.Values)
            {
                if (!composableTypeInfo.IsComposable) continue;
                composableTypeInfo.PropagateConstructionLimitations();
            }
        }
    }
}