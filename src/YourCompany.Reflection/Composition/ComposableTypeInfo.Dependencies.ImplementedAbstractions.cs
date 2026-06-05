using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo
    {
        private HashSet<Type> _implementedComposableAbstractions;

        private void VisitComposableAbstractions(Action<Type> abstractionVisitor)
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            VisitTypeAbstractions(abstractionVisitor);
        }

        private void VisitTypeAbstractions(Action<Type> abstractionVisitor)
            => TypeAbstractionsHelper.VisitTypeAbstractions(Type, abstractionVisitor);

        private void TryRegisterComposableAbstraction(Type abstractionType)
        {
            if (abstractionType == null) throw new ArgumentNullException(nameof(abstractionType));
            if (_types.TryGetValue(abstractionType, out ComposableTypeInfo abstractionTypeInfo))
            {
                if (abstractionType.IsClass && !abstractionType.IsAbstract)
                    AddConstructionLimitationReason(
                        new CompositionException.ProvidedComposableBaseTypesMustBeAbstract(this, abstractionTypeInfo));

                _implementedComposableAbstractions = _implementedComposableAbstractions ?? new HashSet<Type>();
                if (!_implementedComposableAbstractions.Add(abstractionType)) throw new ApplicationException("!_implementedComposableAbstractions.Add(abstractionType)");
                abstractionTypeInfo.AddDependency(Type, this);
            }
        }

        private bool CheckIsComposableAbstraction()
        {
            bool isAbstraction = TypeAbstractionsHelper.CheckIsAbstraction(Type);
            if (isAbstraction && IsComposable) throw new ApplicationException("isAbstraction && IsComposable");
            return isAbstraction;
        }
    }
}