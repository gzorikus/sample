using System;
using YourCompany.Configuration.EFCore.ExpressionsCaching;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching
{
    internal static partial class SortingKeyQueriesCache
    {
        internal static partial class ForQuery<T>
        {
            private sealed partial class ForTopology
            {
                private abstract partial class TopologyVisitor
                {
                    internal abstract partial class MultiEFProperties
                    {
                        internal sealed class ForEntity : MultiEFProperties
                        {
                            private readonly string _replaceQueriedSetName;

                            internal ForEntity(
                                SortingKeyTopology.ILastProperty singleTopology, string replaceQueriedSetName)
                                : base(singleTopology)
                            {
                                singleTopology.VisitPrefixFirst(this);

                                if (_visitedPropertiesTotal != _singleTopologyPropertiesCount)
                                    throw new ApplicationException("_visitedPropertiesTotal != _singleTopologyPropertiesCount");

                                BuildMultiEFPropertySelectValuesExpression();
                                CollectCreateSortingKeyChain(singleTopology);

                                _replaceQueriedSetName = replaceQueriedSetName;
                            }

                            protected override void CollectSelectValuesExpressionVisit<TProperty>(
                                SortingKeyTopology.ILastProperty property)
                            {
                                if (property == null) throw new ArgumentNullException(nameof(property));
                                if (_selectValuesExpression != null) throw new ApplicationException("_selectValuesExpression != null");
                                _multiEFPropertyExpressions[property.PrefixKeysCount]
                                    = EFPropertyExpressionsCache.EFProperty<TProperty>.EntityParameter(
                                        EFPropertyExpressionsCache.FromParameter<T>.Instance,
                                        property.PropertyName);
                            }

                            protected override void ConfigureCreatedSortingKeyQueryOptions(SortingKey sortingKey)
                            {
                                sortingKey.ReplaceQueriedSetName = _replaceQueriedSetName ?? sortingKey.ReplaceQueriedSetName;
                            }
                        }
                    }
                }
            }
        }
    }
}