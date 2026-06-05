using System;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class ProvidedComposableBaseTypesMustBeAbstract : CompositionException
        {
            public ComposableTypeInfo BaseTypeInfo { get; }

            internal ProvidedComposableBaseTypesMustBeAbstract(
                ComposableTypeInfo composableTypeInfo, ComposableTypeInfo baseTypeInfo)
                : base(composableTypeInfo)
            {
                BaseTypeInfo = baseTypeInfo ?? throw new ArgumentNullException(nameof(baseTypeInfo));
            }

            internal ProvidedComposableBaseTypesMustBeAbstract(ProvidedComposableBaseTypesMustBeAbstract prototype)
                : base(prototype)
            {
                if (prototype == null) throw new ArgumentNullException(nameof(prototype));
                BaseTypeInfo = prototype.BaseTypeInfo ?? throw new ApplicationException("prototype.BaseTypeInfo == null");
            }

            protected internal override CompositionException CloneForThrowing()
                => new ProvidedComposableBaseTypesMustBeAbstract(this);
        }
    }
}