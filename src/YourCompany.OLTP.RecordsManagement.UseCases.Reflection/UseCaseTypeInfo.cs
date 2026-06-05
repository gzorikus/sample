using System;
using System.Collections.Generic;
using YourCompany.Reflection;

namespace YourCompany.OLTP.RecordsManagement.UseCases.Reflection
{
    public sealed class UseCaseTypeInfo
    {
        private readonly IDictionary<Type, UseCaseTypeInfo> _types;
        private HashSet<Type> _implementedUseCases;

        public UseCaseTypesMap Map { get; }
        public Type Type { get; }
        public bool IsUseCasesImplementor => _implementedUseCases != null;
        public bool IsUsedAsUseCaseTypeGenericArgumentByAnyImplementor { get; private set; }
        public IEnumerable<Type> ImplementedUseCases => IsUseCasesImplementor ? _implementedUseCases : null;

        internal UseCaseTypeInfo(
            UseCaseTypesMap map, Type possiblyUseCasesImplementorType, IDictionary<Type, UseCaseTypeInfo> types)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Type = possiblyUseCasesImplementorType ?? throw new ArgumentNullException(nameof(possiblyUseCasesImplementorType));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _types.Add(possiblyUseCasesImplementorType, this);
            if (!TypeAbstractionsHelper.CheckIsAbstraction(possiblyUseCasesImplementorType)
                && !possiblyUseCasesImplementorType.IsGenericType)
            {
                TypeAbstractionsHelper.VisitTypeAbstractions(possiblyUseCasesImplementorType, TryCollectImplementedUseCases);
            }
        }

        private void TryCollectImplementedUseCases(Type abstractionType)
        {
            if (Type.IsGenericType) throw new ApplicationException("Type.IsGenericType");
            if (abstractionType.IsGenericType)
            {
                TryCollectSingleArgumentContravariantGenericUseCaseImplementor(abstractionType);
            }
            else
            {
                TryCollectNonGenericUseCaseImplementor(abstractionType);
            }
        }

        private void TryCollectNonGenericUseCaseImplementor(Type abstractionType)
        {
            if (abstractionType == null) throw new ArgumentNullException(nameof(abstractionType));
            if (abstractionType.IsGenericType) throw new ApplicationException("abstractionType.IsGenericType");
            if (Map.ExpectsNonGenericUseCaseType(abstractionType))
            {
                _implementedUseCases = _implementedUseCases ?? new HashSet<Type>();
                if (!_implementedUseCases.Add(abstractionType)) return;
            }
        }

        private void TryCollectSingleArgumentContravariantGenericUseCaseImplementor(Type abstractionType)
        {
            if (abstractionType == null) throw new ArgumentNullException(nameof(abstractionType));
            if (!abstractionType.IsGenericType) throw new ApplicationException("!abstractionType.IsGenericType");

            var abstractionGenericTypeDefinition = abstractionType.GetGenericTypeDefinition();
            if (!Map.ExpectsSingleArgumentContravariantUseCaseGenericTypeDefinition(abstractionGenericTypeDefinition))
                return;

            _implementedUseCases = _implementedUseCases ?? new HashSet<Type>();
            if (!_implementedUseCases.Add(abstractionType)) return;

            var potentialAbstraction = abstractionType.GetGenericArguments()[0];
            if (_types.TryGetValue(potentialAbstraction, out var potentialAbstractionTypeInfo))
                potentialAbstractionTypeInfo.IsUsedAsUseCaseTypeGenericArgumentByAnyImplementor = true;

            if (potentialAbstraction.IsSealed) return;

            VisitDerivingDomainTypes(potentialAbstraction, derivedType =>
            {
                _implementedUseCases.Add(abstractionGenericTypeDefinition.MakeGenericType(derivedType));
                if (_types.TryGetValue(derivedType, out var derivedTypeInfo))
                    derivedTypeInfo.IsUsedAsUseCaseTypeGenericArgumentByAnyImplementor = true;
            });
        }

        private void VisitDerivingDomainTypes(Type sourceType, Action<Type> derivingTypeVisitor)
        {
            foreach (var potentiallyDerivingType in _types.Keys)
                if (sourceType != potentiallyDerivingType && sourceType.IsAssignableFrom(potentiallyDerivingType))
                    derivingTypeVisitor(potentiallyDerivingType);
        }
    }
}