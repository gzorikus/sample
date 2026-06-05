using System;
using System.Collections.Generic;
using System.Reflection;

namespace YourCompany.Reflection.Composition
{
    public abstract class ComposableTypesProvider
    {
        public abstract IEnumerable<Type> GetTypesToMap();

        public abstract IReadOnlyList<ParameterInfo> TryGetComposableConstructorParameters(
            ComposableTypeInfo composableTypeInfo, Action<CompositionException> constructionLimitationReasonsCollector);

        public virtual bool CheckIsCrossCuttingMixin(ComposableTypeInfo composableTypeInfo) => false;

        public virtual bool CheckToOmitCrossCuttingMixinForRoot(
            ComposableTypeInfo crossCuttingMixin, ComposableTypeInfo rootWithoutDeterminedMixins)
            => false;

        public virtual ConstructorInfo TryGetSingleComposableConstructor(
            ComposableTypeInfo composableTypeInfo, IReadOnlyList<ParameterInfo> composableConstructorParameters)
            => null;
    }
}