using System;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.Reflection.Composition;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection
{
    public abstract class RecordCompositionException : CompositionException
    {
        private RecordCompositionException(ComposableTypeInfo composableTypeInfo) : base(composableTypeInfo) { }

        private RecordCompositionException(RecordCompositionException prototype) : base(prototype?.ComposableTypeInfo)
        {
            if (prototype == null) throw new ArgumentNullException(nameof(prototype));
        }

        public sealed class HasRecordConstructionLimitations : RecordCompositionException
        {
            public RecordConstructionException RecordConstructionException { get; }

            internal HasRecordConstructionLimitations(
                ComposableTypeInfo composableTypeInfo, RecordConstructionException recordConstructionException)
                : base(composableTypeInfo)
            {
                RecordConstructionException = recordConstructionException ?? throw new ArgumentNullException(nameof(recordConstructionException));
            }

            internal HasRecordConstructionLimitations(HasRecordConstructionLimitations prototype) : base(prototype)
            {
                RecordConstructionException = prototype.RecordConstructionException ?? throw new ApplicationException("prototype.RecordConstructionException == null");
            }

            protected override CompositionException CloneForThrowing() => new HasRecordConstructionLimitations(this);
        }
    }
}