namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class MixinMustHaveApplicableRoots : CompositionException
        {
            internal MixinMustHaveApplicableRoots(ComposableTypeInfo mixinTypeInfo) : base(mixinTypeInfo) { }
            internal MixinMustHaveApplicableRoots(MixinMustHaveApplicableRoots prototype) : base(prototype) { }
            protected internal override CompositionException CloneForThrowing() => new MixinMustHaveApplicableRoots(this);
        }
    }
}