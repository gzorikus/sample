using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public abstract partial class CompositionException
    {
        public sealed class Composite : CompositionException
        {
            public IReadOnlyList<CompositionException> Reasons { get; }

            internal Composite(CompositionException original, CompositionException additional)
                : base(original.ComposableTypeInfo)
            {
                List<CompositionException> reasons;
                IReadOnlyList<CompositionException> newReasons = null;

                if (original is Composite originalComposite)
                {
                    if (additional is Composite additionalComposite)
                    {
                        reasons = new List<CompositionException>(
                            originalComposite.Reasons.Count + additionalComposite.Reasons.Count);
                        reasons.AddRange(originalComposite.Reasons);
                        newReasons = additionalComposite.Reasons;
                    }
                    else
                    {
                        reasons = new List<CompositionException>(originalComposite.Reasons.Count + 1);
                        reasons.AddRange(originalComposite.Reasons);
                    }
                }
                else if (additional is Composite additionalComposite)
                {
                    reasons = new List<CompositionException>(1 + additionalComposite.Reasons.Count);
                    reasons.Add(original);
                    newReasons = additionalComposite.Reasons;
                }
                else
                {
                    reasons = new List<CompositionException>(2) { original };
                }

                for (int i = 0; i < (newReasons?.Count ?? 1); i++)
                {
                    var newReason = newReasons?[i] ?? additional ?? throw new ApplicationException("newReason == null");

                    if (newReason is DependenciesLoopDetected newLoopDetected)
                    {
                        for (int j = 0; j < reasons.Count; j++)
                        {
                            if (reasons[j] is DependenciesLoopDetected prevLoopDetected)
                            {
                                for (int k = 0; k < newLoopDetected.Path.Count; k++)
                                    prevLoopDetected.AddToPath(newLoopDetected.Path[k]);
                                newReason = null;
                                break;
                            }
                        }
                    }

                    if (newReason != null)
                        reasons.Add(newReason);
                }

                Reasons = reasons;
            }

            internal Composite(Composite prototype) : base(prototype)
            {
                var reasons = new CompositionException[Reasons.Count];

                for (int i = 0; i < reasons.Length; i++)
                    reasons[i] = Reasons[i].CloneForThrowing();

                Reasons = reasons;
            }

            public bool CheckHasConstructionLimitationReason(Type compositionExceptionType)
            {
                if (compositionExceptionType == null)
                    throw new ArgumentNullException(nameof(compositionExceptionType));

                for (int i = 0; i < Reasons.Count; i++)
                    if (compositionExceptionType.IsAssignableFrom(Reasons[i].GetType()))
                        return true;

                return false;
            }

            public bool TryGetDependenciesLoop(out DependenciesLoopDetected dependenciesLoop)
            {
                DependenciesLoopDetected singleReason = null;

                for (int i = 0; i < Reasons.Count; i++)
                {
                    if (Reasons[i] is DependenciesLoopDetected reason)
                    {
                        if (singleReason != null)
                            throw new ApplicationException("singleReason != null");

                        singleReason = reason;
                    }
                }

                dependenciesLoop = singleReason;
                return dependenciesLoop != null;
            }

            protected internal override CompositionException CloneForThrowing() => new Composite(this);
        }
    }
}