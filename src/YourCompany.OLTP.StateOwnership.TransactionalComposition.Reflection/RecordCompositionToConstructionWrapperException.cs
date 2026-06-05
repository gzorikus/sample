using System;
using System.Reflection;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.Reflection.Composition;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection
{
    public sealed class RecordCompositionToConstructionWrapperException : RecordConstructionException
    {
        private readonly Func<CompositionException> _cloneCompositionExceptionForThrowing;

        public CompositionException CompositionException { get; }

        internal RecordCompositionToConstructionWrapperException(
            Func<CompositionException> cloneCompositionExceptionForThrowing, ConstructorInfo constructorInfo)
            : base(constructorInfo)
        {
            _cloneCompositionExceptionForThrowing = cloneCompositionExceptionForThrowing ?? throw new ArgumentNullException(nameof(cloneCompositionExceptionForThrowing));
            CompositionException = cloneCompositionExceptionForThrowing() ?? throw new ApplicationException("compositionException == null");
        }

        internal RecordCompositionToConstructionWrapperException(
            RecordCompositionToConstructionWrapperException prototype)
            : base(prototype)
        {
            _cloneCompositionExceptionForThrowing = prototype._cloneCompositionExceptionForThrowing ?? throw new ApplicationException("prototype._cloneCompositionExceptionForThrowing == null");
            CompositionException = _cloneCompositionExceptionForThrowing() ?? throw new ApplicationException("compositionException == null");
        }

        protected override RecordConstructionException CloneForThrowing()
            => new RecordCompositionToConstructionWrapperException(this);
    }
}