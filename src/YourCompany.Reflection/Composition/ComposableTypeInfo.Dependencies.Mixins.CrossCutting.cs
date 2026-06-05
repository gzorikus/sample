using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo
    {
        private HashSet<ComposableTypeInfo> _crossCuttingMixinOmittedForRoots;

        private void CollectCrossCuttingMixinDependencyApplicableRoot(
            ComposableTypeInfo dependencyComposableTypeInfo, ComposableTypeInfo rootTypeInfo)
        {
            if (dependencyComposableTypeInfo == null) throw new ArgumentNullException(nameof(dependencyComposableTypeInfo));
            if (rootTypeInfo == null) throw new ArgumentNullException(nameof(rootTypeInfo));
            if (!dependencyComposableTypeInfo.IsComposable) throw new ApplicationException("!dependencyComposableTypeInfo.IsComposable");
            if (!rootTypeInfo.IsComposable || rootTypeInfo.IsMixin) throw new ApplicationException("!rootTypeInfo.IsComposable || rootTypeInfo.IsMixin");
            if (!IsCrossCuttingMixin || _applicableRoots == null) throw new ApplicationException("!IsCrossCuttingMixin || _applicableRoots == null");

            if (dependencyComposableTypeInfo.IsCrossCuttingMixin)
            {
                if (dependencyComposableTypeInfo._applicableRoots == null)
                    throw new ApplicationException("dependencyComposableTypeInfo._applicableRoots == null");

                if (!dependencyComposableTypeInfo.CheckRootWasCollectedByCrossCuttingMixin(rootTypeInfo))
                    dependencyComposableTypeInfo.CollectCrossCuttingMixinApplicableRoots(rootTypeInfo);

                if (dependencyComposableTypeInfo._applicableRoots.Contains(rootTypeInfo))
                {
                    if (dependencyComposableTypeInfo._crossCuttingMixinOmittedForRoots.Contains(rootTypeInfo))
                        throw new ApplicationException("dependencyComposableTypeInfo._crossCuttingMixinOmittedForRoots.Contains(rootTypeInfo)");

                    _applicableRoots.Add(rootTypeInfo);
                }
                else
                {
                    if (dependencyComposableTypeInfo._crossCuttingMixinOmittedForRoots?.Contains(rootTypeInfo) != true)
                        throw new ApplicationException("dependencyComposableTypeInfo._crossCuttingMixinOmittedForRoots?.Contains(rootTypeInfo) != true");

                    _crossCuttingMixinOmittedForRoots = _crossCuttingMixinOmittedForRoots
                        ?? new HashSet<ComposableTypeInfo>();
                    _crossCuttingMixinOmittedForRoots.Add(rootTypeInfo);
                }

                PropagateComposableTypeDependenciesLoop(dependencyComposableTypeInfo);
            }
            else
            {
                if (dependencyComposableTypeInfo.CheckComposableHasConstructionLimitationReason(
                    typeof(CompositionException.DependenciesLoopDetected)))
                    throw new ApplicationException("dependencyComposableTypeInfo.CheckComposableHasConstructionLimitationReason(typeof(CompositionException.DependenciesLoopDetected))");

                AddConstructionLimitationReason(
                    new CompositionException.CrossCuttingMixinMustOnlyDependOnOtherCrossCutting(
                        this, dependencyComposableTypeInfo));
            }
        }

        private void DetermineCrossCuttingMixinAsApplicableToRoot(ComposableTypeInfo rootTypeInfo)
        {
            if (rootTypeInfo == null) throw new ArgumentNullException(nameof(rootTypeInfo));
            if (!rootTypeInfo.IsComposable || rootTypeInfo.IsMixin) throw new ApplicationException("!rootTypeInfo.IsComposable || rootTypeInfo.IsMixin");
            if (!IsCrossCuttingMixin || _applicableRoots == null) throw new ApplicationException("!IsCrossCuttingMixin || _applicableRoots == null");

            if (_crossCuttingMixinOmittedForRoots != null && _crossCuttingMixinOmittedForRoots.Contains(rootTypeInfo))
            {
                _applicableRoots.Remove(rootTypeInfo);
            }
            else if (Provider.CheckToOmitCrossCuttingMixinForRoot(this, rootTypeInfo))
            {
                _applicableRoots.Remove(rootTypeInfo);
                _crossCuttingMixinOmittedForRoots = _crossCuttingMixinOmittedForRoots ?? new HashSet<ComposableTypeInfo>();
                if (!_crossCuttingMixinOmittedForRoots.Add(rootTypeInfo))
                    throw new ApplicationException("!_crossCuttingMixinOmittedForRoots.Add(rootTypeInfo)");
            }
            else
            {
                _applicableRoots.Add(rootTypeInfo);
                rootTypeInfo.AddRootOrderedMixin(this);
            }
        }
    }
}