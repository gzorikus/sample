using System;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable
            : CompositionException
        {
            public ComposableTypeInfo DependencyComposableTypeInfo { get; }

            internal AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable(
                ComposableTypeInfo composableTypeInfo, ComposableTypeInfo dependencyComposableTypeInfo)
                : base(composableTypeInfo)
            {
                DependencyComposableTypeInfo = dependencyComposableTypeInfo ?? throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            }

            internal AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable(
                AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable prototype)
                : base(prototype)
            {
                if (prototype == null) throw new ArgumentNullException(nameof(prototype));
                DependencyComposableTypeInfo = prototype.DependencyComposableTypeInfo ?? throw new ApplicationException("prototype.DependencyComposableTypeInfo == null");
            }

            protected internal override CompositionException CloneForThrowing()
                => new AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable(this);
        }
    }
}