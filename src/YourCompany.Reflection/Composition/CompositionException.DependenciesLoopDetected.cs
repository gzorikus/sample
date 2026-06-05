using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class DependenciesLoopDetected : CompositionException
        {
            private readonly List<ComposableTypeInfo> _path;

            public int VisitingComposableConstructorParameterIndex { get; }
            public IReadOnlyList<ComposableTypeInfo> Path => _path;

            internal DependenciesLoopDetected(
                ComposableTypeInfo composableTypeInfo, int visitingComposableConstructorParameterIndex)
                : base(composableTypeInfo)
            {
                VisitingComposableConstructorParameterIndex = visitingComposableConstructorParameterIndex;
                _path = new List<ComposableTypeInfo>();
            }

            internal DependenciesLoopDetected(DependenciesLoopDetected prototype) : base(prototype)
            {
                if (prototype == null) throw new ArgumentNullException(nameof(prototype));
                _path = prototype._path ?? throw new ApplicationException("prototype._path == null");
                VisitingComposableConstructorParameterIndex = prototype.VisitingComposableConstructorParameterIndex;
            }

            internal void AddToPath(ComposableTypeInfo composableTypeInfo)
            {
                if (composableTypeInfo == null) throw new ArgumentNullException(nameof(composableTypeInfo));
                _path.Add(composableTypeInfo);
            }

            protected internal override CompositionException CloneForThrowing() => new DependenciesLoopDetected(this);
        }
    }
}