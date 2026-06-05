using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo
    {
        private void DetermineAsMixinForAnyDependencyComposableExceptCrossCutting(
            ComposableTypeInfo dependencyComposableTypeInfo)
        {
            if (dependencyComposableTypeInfo == null) throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            if (!IsComposable || IsCrossCuttingMixin) throw new ApplicationException("!IsComposable || IsCrossCuttingMixin");
            if (!dependencyComposableTypeInfo.IsCrossCuttingMixin)
                _applicableRoots = new HashSet<ComposableTypeInfo>();
        }

        private void CollectDeterminedMixinDependencyApplicableRoots(ComposableTypeInfo dependencyComposableTypeInfo)
        {
            if (dependencyComposableTypeInfo == null) throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            if (!IsMixin || IsCrossCuttingMixin) throw new ApplicationException("!IsMixin || IsCrossCuttingMixin");
            if (!dependencyComposableTypeInfo.IsComposable) throw new ApplicationException("!dependencyComposableTypeInfo.IsComposable");
            if (_applicableRoots == null) throw new ApplicationException("_applicableRoots == null");
            if (_visitingComposableConstructorParameterDependency == null) throw new ApplicationException("_visitingComposableConstructorParameterDependency == null");

            if (dependencyComposableTypeInfo.IsMixin &&
                !dependencyComposableTypeInfo.IsCrossCuttingMixin &&
                dependencyComposableTypeInfo._applicableRoots.Count == 0 &&
                dependencyComposableTypeInfo._constructionLimitationReason == null)
                dependencyComposableTypeInfo.CollectDeterminedMixinsApplicableRoots();

            if (dependencyComposableTypeInfo.IsMixin)
            {
                if (dependencyComposableTypeInfo._applicableRoots == null) throw new ApplicationException("dependencyComposableTypeInfo._applicableRoots == null");
                _applicableRoots.UnionWith(dependencyComposableTypeInfo._applicableRoots);
                if (_visitingComposableConstructorParameterDependency.CheckIsComposableAbstraction())
                {
                    _visitingComposableConstructorParameterDependency._applicableRoots
                        = _visitingComposableConstructorParameterDependency._applicableRoots
                            ?? new HashSet<ComposableTypeInfo>();
                    _visitingComposableConstructorParameterDependency._applicableRoots.UnionWith(
                        dependencyComposableTypeInfo._applicableRoots);
                }
            }
            else
            {
                _applicableRoots.Add(dependencyComposableTypeInfo);
                if (_visitingComposableConstructorParameterDependency.CheckIsComposableAbstraction())
                {
                    _visitingComposableConstructorParameterDependency._applicableRoots
                        = _visitingComposableConstructorParameterDependency._applicableRoots
                            ?? new HashSet<ComposableTypeInfo>();
                    _visitingComposableConstructorParameterDependency._applicableRoots.Add(dependencyComposableTypeInfo);
                }
            }

            PropagateComposableTypeDependenciesLoop(dependencyComposableTypeInfo);
        }

        private void DetermineMixinApplicabilityAmongCollectedDependencyRoots()
        {
            if (!IsMixin || IsCrossCuttingMixin) throw new ApplicationException("!IsMixin || IsCrossCuttingMixin");
            if (_applicableRoots == null) throw new ApplicationException("_applicableRoots == null");

            VisitComposableConstructorParameterDependencies(dependencyTypeInfo =>
            {
                if (dependencyTypeInfo.CheckIsComposableAbstraction() || dependencyTypeInfo.IsMixin)
                {
                    if (dependencyTypeInfo._applicableRoots == null)
                    {
                        _applicableRoots.Clear();
                    }
                    else
                    {
                        _applicableRoots.IntersectWith(dependencyTypeInfo._applicableRoots);
                    }
                }
                else
                {
                    if (!dependencyTypeInfo.IsComposable || dependencyTypeInfo._applicableRoots != null)
                        throw new ApplicationException("!dependencyTypeInfo.IsComposable || dependencyTypeInfo._applicableRoots != null");

                    _applicableRoots.Clear();
                    _applicableRoots.Add(dependencyTypeInfo);
                }
            });

            if (MixinToSingleRoot != null) throw new ApplicationException("MixinToSingleRoot != null");
            if (_applicableRoots.Count > 0)
            {
                foreach (var mixinApplicableRootTypeInfo in _applicableRoots)
                {
                    if (_applicableRoots.Count == 1)
                        MixinToSingleRoot = mixinApplicableRootTypeInfo;

                    mixinApplicableRootTypeInfo.AddRootOrderedMixin(this);
                }
            }
            else
            {
                AddConstructionLimitationReason(new CompositionException.MixinMustHaveApplicableRoots(this));
            }
        }
    }
}