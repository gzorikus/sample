using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public partial class ComposableTypeInfo
    {
        private CompositionException _constructionLimitationReason;

        public bool CheckComposableHasConstructionLimitationReason(Type compositionExceptionType)
        {
            if (compositionExceptionType == null) throw new ArgumentNullException(nameof(compositionExceptionType));
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (!typeof(CompositionException).IsAssignableFrom(compositionExceptionType))
                throw new ApplicationException("!typeof(CompositionException).IsAssignableFrom(compositionExceptionType)");

            if (_constructionLimitationReason == null) return false;
            return _constructionLimitationReason is CompositionException.Composite composite
                ? composite.CheckHasConstructionLimitationReason(compositionExceptionType)
                : compositionExceptionType.IsAssignableFrom(_constructionLimitationReason.GetType());
        }

        public CompositionException GetCompositionExceptionForThrowing()
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (_constructionLimitationReason == null) throw new ApplicationException("_constructionLimitationReason == null");
            return _constructionLimitationReason.CloneForThrowing() ?? throw new ApplicationException("clonedException == null");
        }

        internal void PropagateConstructionLimitations()
            => VisitComposableDependencyComposables(
                TryLimitCompositionDependenciesFromMultipleImplementationsReferencingWithoutEnumeration);

        private void TryLimitCompositionDependenciesFromMultipleImplementationsReferencingWithoutEnumeration(
            ComposableTypeInfo dependencyComposableTypeInfo)
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (dependencyComposableTypeInfo == null) throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            if (!dependencyComposableTypeInfo.IsComposable) throw new ApplicationException("!dependencyComposableTypeInfo.IsComposable");
            if (_visitingComposableConstructorParameterDependency == null) throw new ApplicationException("_visitingComposableConstructorParameterDependency == null");
            int parameterIndex = _visitingComposableConstructorParameterIndex ?? throw new ApplicationException("!_visitingComposableConstructorParameterIndex.HasValue");
            if (!_visitingComposableConstructorParameterDependency.CheckIsComposableAbstraction()) return;

            var parameterType = _composableConstructorParameters[parameterIndex].ParameterType;
            if (_visitingComposableConstructorParameterDependency.Type != parameterType)
            {
                if (_visitingComposableConstructorParameterDependency.Type
                    != typeof(IEnumerable<>).MakeGenericType(parameterType))
                    throw new ApplicationException("_visitingComposableConstructorParameterDependency.Type != typeof(IEnumerable<>).MakeGenericType(parameterType)");

                return;
            }

            CompositionException.AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable reason = null;

            foreach (var rootTypeInfo in EnumerateDependencyRoots())
            {
                int implementationsCount = 0;
                if (_visitingComposableConstructorParameterDependency.Type.IsAssignableFrom(rootTypeInfo.Type))
                    implementationsCount++;

                if (rootTypeInfo._orderedCrossCuttingMixinsFollowedByRootFollowedByMixins == null)
                    throw new ApplicationException("rootTypeInfo._orderedCrossCuttingMixinsFollowedByRootFollowedByMixins == null");

                bool rootFound = false;
                for (int i = 0; i < rootTypeInfo._orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Count; i++)
                {
                    var rootOrderedTypeInfo = rootTypeInfo._orderedCrossCuttingMixinsFollowedByRootFollowedByMixins[i];
                    if (rootTypeInfo == rootOrderedTypeInfo)
                    {
                        if (rootFound) throw new ApplicationException("rootFound");
                        rootFound = true;
                    }
                    else
                    {
                        if (_visitingComposableConstructorParameterDependency.Type.IsAssignableFrom(rootOrderedTypeInfo.Type))
                            implementationsCount++;
                    }
                }

                if (implementationsCount > 1)
                {
                    reason = new CompositionException
                        .AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable(
                            this, dependencyComposableTypeInfo);
                    AddConstructionLimitationReason(reason);
                }
            }

            if (reason != null)
                foreach (var rootTypeInfo in EnumerateDependencyRoots())
                    rootTypeInfo.AddConstructionLimitationReason(reason);

            IEnumerable<ComposableTypeInfo> EnumerateDependencyRoots()
            {
                if (dependencyComposableTypeInfo.IsMixin)
                {
                    if (dependencyComposableTypeInfo._applicableRoots == null)
                        throw new ApplicationException("dependencyComposableTypeInfo._applicableRoots == null");

                    foreach (var rootTypeInfo in dependencyComposableTypeInfo._applicableRoots)
                        yield return rootTypeInfo;
                }
                else
                {
                    yield return dependencyComposableTypeInfo;
                }
            }
        }

        private void PropagateComposableTypeDependenciesLoop(ComposableTypeInfo dependencyComposableTypeInfo)
        {
            if (dependencyComposableTypeInfo.TryGetDependenciesLoop(
                out CompositionException.DependenciesLoopDetected detectedLoop))
            {
                detectedLoop.AddToPath(this);
                AddConstructionLimitationReason(detectedLoop);
            }
        }

        private bool TryGetDependenciesLoop(out CompositionException.DependenciesLoopDetected dependenciesLoop)
        {
            dependenciesLoop = _constructionLimitationReason as CompositionException.DependenciesLoopDetected;
            return dependenciesLoop == null && _constructionLimitationReason is CompositionException.Composite composite
                ? composite.TryGetDependenciesLoop(out dependenciesLoop)
                : dependenciesLoop != null;
        }

        private void AddConstructionLimitationReason(CompositionException reason)
        {
            if (_constructionLimitationReason == null)
            {
                _constructionLimitationReason = reason;
            }
            else
            {
                _constructionLimitationReason = new CompositionException.Composite(_constructionLimitationReason, reason);
            }
        }
    }
}