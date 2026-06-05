using System;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException : Exception
    {
        public ComposableTypeInfo ComposableTypeInfo { get; }

        protected CompositionException(ComposableTypeInfo composableTypeInfo) : base(null)
        {
            if (composableTypeInfo == null) throw new ArgumentNullException(nameof(composableTypeInfo));
            if (!composableTypeInfo.IsComposable) throw new ApplicationException("!composableTypeInfo.IsComposable");
            ComposableTypeInfo = composableTypeInfo;
        }

        protected CompositionException(CompositionException prototype) : base(null)
        {
            if (prototype == null) throw new ArgumentNullException(nameof(prototype));
            ComposableTypeInfo = prototype.ComposableTypeInfo ?? throw new ApplicationException("prototype.ComposableTypeInfo == null");
        }

        protected internal abstract CompositionException CloneForThrowing();
    }
}