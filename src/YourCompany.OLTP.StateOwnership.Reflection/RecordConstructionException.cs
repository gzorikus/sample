using System;
using System.Reflection;

namespace YourCompany.OLTP.StateOwnership.Reflection
{
    public abstract class RecordConstructionException : Exception
    {
        public ConstructorInfo ConstructorInfo { get; }

        protected RecordConstructionException(ConstructorInfo constructorInfo) : base(null)
        {
            ConstructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        }

        protected RecordConstructionException(RecordConstructionException prototype) : base(null)
        {
            if (prototype == null) throw new ArgumentNullException(nameof(prototype));
            ConstructorInfo = prototype.ConstructorInfo ?? throw new ApplicationException("prototype.ConstructorInfo == null");
        }

        protected internal abstract RecordConstructionException CloneForThrowing();

        public sealed class ConstructorMustHaveSingleStateAccessParameter : RecordConstructionException
        {
            internal ConstructorMustHaveSingleStateAccessParameter(ConstructorInfo constructorInfo)
                : base(constructorInfo) { }

            internal ConstructorMustHaveSingleStateAccessParameter(ConstructorMustHaveSingleStateAccessParameter prototype)
                : base(prototype) { }

            protected internal override RecordConstructionException CloneForThrowing()
                => new ConstructorMustHaveSingleStateAccessParameter(this);
        }

        public sealed class ConstructorMustBeInternal : RecordConstructionException
        {
            internal ConstructorMustBeInternal(ConstructorInfo constructorInfo) : base(constructorInfo) { }
            internal ConstructorMustBeInternal(ConstructorMustBeInternal prototype) : base(prototype) { }
            protected internal override RecordConstructionException CloneForThrowing() => new ConstructorMustBeInternal(this);
        }

        public sealed class ConstructorMustBeSingle : RecordConstructionException
        {
            public ConstructorInfo ExtraRecordConstructorInfo { get; }

            internal ConstructorMustBeSingle(ConstructorInfo constructorInfo, ConstructorInfo extraRecordConstructorInfo)
                : base(constructorInfo)
                => ExtraRecordConstructorInfo = extraRecordConstructorInfo ?? throw new ArgumentNullException(nameof(extraRecordConstructorInfo));

            internal ConstructorMustBeSingle(ConstructorMustBeSingle prototype) : base(prototype)
                => ExtraRecordConstructorInfo = prototype.ExtraRecordConstructorInfo ?? throw new ApplicationException("prototype.ExtraRecordConstructorInfo == null");

            protected internal override RecordConstructionException CloneForThrowing() => new ConstructorMustBeSingle(this);
        }
    }
}