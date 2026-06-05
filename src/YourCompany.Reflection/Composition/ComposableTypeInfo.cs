using System;
using System.Collections.Generic;
using System.Reflection;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo
    {
        private readonly IDictionary<Type, ComposableTypeInfo> _types;
        private readonly IReadOnlyList<ParameterInfo> _composableConstructorParameters;
        private List<ComposableTypeInfo> _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins;

        public TypesCompositionMap Map { get; }
        public ComposableTypesProvider Provider { get; }
        public Type Type { get; }
        public bool IsComposable => _composableConstructorParameters != null;
        public bool IsRoot => IsComposable && !IsMixin;
        public bool IsMixin => _applicableRoots != null;
        public ComposableTypeInfo MixinToSingleRoot { get; private set; }
        public bool IsCrossCuttingMixin { get; }
        public bool IsImplementedComposableAbstraction => !IsComposable && GetDependenciesReadonly().Count > 0;

        public IReadOnlyList<ComposableTypeInfo> RootOrderedComposables => IsRoot
            ? _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins
            : null;

        public IEnumerable<ComposableTypeInfo> MixinApplicableRoots => IsMixin ? _applicableRoots : null;

        public IEnumerable<ComposableTypeInfo> CrossCuttingMixinOmittedForRoots => IsCrossCuttingMixin
            ? (IEnumerable<ComposableTypeInfo>)_crossCuttingMixinOmittedForRoots ?? Array.Empty<ComposableTypeInfo>()
            : null;

        public IEnumerable<Type> ImplementedComposableAbstractions => IsComposable
            ? (IEnumerable<Type>)_implementedComposableAbstractions ?? Array.Empty<Type>()
            : null;

        internal ComposableTypeInfo(
            TypesCompositionMap map,
            ComposableTypesProvider provider,
            Type possiblyComposableType,
            IDictionary<Type, ComposableTypeInfo> types)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Type = possiblyComposableType ?? throw new ArgumentNullException(nameof(possiblyComposableType));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _types.Add(possiblyComposableType, this);
            if (!CheckIsComposableAbstraction())
            {
                _composableConstructorParameters = provider.TryGetComposableConstructorParameters(
                    this, AddConstructionLimitationReason);
                if (_composableConstructorParameters != null)
                {
                    IsCrossCuttingMixin = provider.CheckIsCrossCuttingMixin(this);
                    if (IsCrossCuttingMixin) _applicableRoots = new HashSet<ComposableTypeInfo>();
                }
            }
        }

        public bool TryGetComposableSingleConstructor(out ConstructorInfo constructorInfo)
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (_composableConstructorParameters == null) throw new ApplicationException("_composableConstructorParameters == null");
            constructorInfo = Provider.TryGetSingleComposableConstructor(this, _composableConstructorParameters);
            return constructorInfo != null;
        }

        public bool CheckRootOrderedComposablesContain(Type possiblyComposableType)
        {
            if (possiblyComposableType == null) throw new ArgumentNullException(nameof(possiblyComposableType));
            if (!IsRoot) throw new ApplicationException("!IsRoot");
            if (Type == possiblyComposableType) return true;
            return _types.TryGetValue(possiblyComposableType, out ComposableTypeInfo possiblyMixinTypeInfo)
                && possiblyMixinTypeInfo.IsMixin && possiblyMixinTypeInfo._applicableRoots.Contains(this);
        }

        internal void CollectComposableConstructorDependenciesAndRegisterAbstractionsImplementation()
        {
            VisitComposableConstructorParameters(TryCollectComposableConstructorDependency);
            VisitComposableAbstractions(TryRegisterComposableAbstraction);
        }

        internal void TryDetermineComposableAsMixin()
        {
            if (!IsComposable || IsMixin) throw new ApplicationException("!IsComposable || IsMixin");
            VisitComposableDependencyComposables(DetermineAsMixinForAnyDependencyComposableExceptCrossCutting);
            InitDeterminedComposableTypeSpecificCollections();
        }

        internal bool CheckRootWasCollectedByCrossCuttingMixin(ComposableTypeInfo rootTypeInfo)
        {
            if (rootTypeInfo == null) throw new ArgumentNullException(nameof(rootTypeInfo));
            if (!IsCrossCuttingMixin || _applicableRoots == null) throw new ApplicationException("!IsCrossCuttingMixin || _applicableRoots == null");
            return _applicableRoots.Contains(rootTypeInfo)
                || _crossCuttingMixinOmittedForRoots?.Contains(rootTypeInfo) == true;
        }

        internal void CollectCrossCuttingMixinApplicableRoots(ComposableTypeInfo rootTypeInfo)
        {
            if (rootTypeInfo == null) throw new ArgumentNullException(nameof(rootTypeInfo));
            if (!IsCrossCuttingMixin) throw new ApplicationException("!IsCrossCuttingMixin");
            VisitComposableDependencyComposables(dependencyComposableTypeInfo
                => CollectCrossCuttingMixinDependencyApplicableRoot(dependencyComposableTypeInfo, rootTypeInfo));
            DetermineCrossCuttingMixinAsApplicableToRoot(rootTypeInfo);
        }

        internal void CollectDeterminedMixinsApplicableRoots()
        {
            if (!IsMixin || IsCrossCuttingMixin) throw new ApplicationException("!IsMixin || IsCrossCuttingMixin");
            VisitComposableDependencyComposables(CollectDeterminedMixinDependencyApplicableRoots);
            DetermineMixinApplicabilityAmongCollectedDependencyRoots();
        }

        private void InitDeterminedComposableTypeSpecificCollections()
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (_crossCuttingMixinOmittedForRoots != null
                || _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins != null)
                throw new ApplicationException("_crossCuttingMixinOmittedForRoots != null || _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins != null");

            if (IsMixin)
            {
                if (_applicableRoots == null) throw new ApplicationException("_applicableRoots == null");
                if (IsCrossCuttingMixin) throw new ApplicationException("IsCrossCuttingMixin");
            }
            else
            {
                if (_applicableRoots != null) throw new ApplicationException("_applicableRoots != null");
                _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins = new List<ComposableTypeInfo> { this };
            }
        }
    }
}