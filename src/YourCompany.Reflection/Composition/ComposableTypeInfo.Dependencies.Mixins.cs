using System;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo
    {
        private HashSet<ComposableTypeInfo> _applicableRoots;

        private void AddRootOrderedMixin(ComposableTypeInfo mixinTypeInfo)
        {
            if (mixinTypeInfo == null) throw new ArgumentNullException(nameof(mixinTypeInfo));
            if (!IsRoot) throw new ApplicationException("!IsRoot");
            if (!mixinTypeInfo.IsMixin) throw new ApplicationException("!mixinTypeInfo.IsMixin");
            if (_orderedCrossCuttingMixinsFollowedByRootFollowedByMixins == null) throw new ApplicationException("_orderedCrossCuttingMixinsFollowedByRootFollowedByMixins == null");

            if (mixinTypeInfo.IsCrossCuttingMixin)
            {
                if (_orderedCrossCuttingMixinsFollowedByRootFollowedByMixins[
                    _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Count - 1] != this)
                    throw new ApplicationException("_orderedCrossCuttingMixinsFollowedByRootFollowedByMixins[_orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Count - 1] != this");

                _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Insert(
                    _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Count - 1, mixinTypeInfo);
            }
            else
            {
                _orderedCrossCuttingMixinsFollowedByRootFollowedByMixins.Add(mixinTypeInfo);
            }

            AddDependency(mixinTypeInfo.Type, mixinTypeInfo);
            if (mixinTypeInfo._constructionLimitationReason != null)
                AddConstructionLimitationReason(mixinTypeInfo._constructionLimitationReason);
        }
    }
}