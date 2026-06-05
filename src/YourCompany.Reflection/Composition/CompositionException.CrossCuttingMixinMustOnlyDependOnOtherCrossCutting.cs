using System;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class CrossCuttingMixinMustOnlyDependOnOtherCrossCutting : CompositionException
        {
            public ComposableTypeInfo DependencyComposableTypeInfo { get; }

            internal CrossCuttingMixinMustOnlyDependOnOtherCrossCutting(
                ComposableTypeInfo composableTypeInfo, ComposableTypeInfo dependencyComposableTypeInfo)
                : base(composableTypeInfo)
            {
                DependencyComposableTypeInfo = dependencyComposableTypeInfo ?? throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            }

            internal CrossCuttingMixinMustOnlyDependOnOtherCrossCutting(
                CrossCuttingMixinMustOnlyDependOnOtherCrossCutting prototype)
                : base(prototype)
            {
                if (prototype == null) throw new ArgumentNullException(nameof(prototype));
                DependencyComposableTypeInfo = prototype.DependencyComposableTypeInfo ?? throw new ApplicationException("prototype.DependencyComposableTypeInfo == null");
            }

            protected internal override CompositionException CloneForThrowing()
                => new CrossCuttingMixinMustOnlyDependOnOtherCrossCutting(this);
        }
    }
}