using System;
using System.Linq;
using System.Linq.Expressions;
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
                    internal sealed class SingleEFProperty : TopologyVisitor
                    {
                        private readonly string _replaceQueriedSetName;

                        internal SingleEFProperty(
                            SortingKeyTopology.ILastProperty singleTopology, string replaceQueriedSetName)
                            : base(singleTopology)
                        {
                            if (singleTopology == null) throw new ArgumentNullException(nameof(singleTopology));
                            if (_singleTopologyPropertiesCount != 1) throw new ApplicationException("_singleTopologyPropertiesCount != 1");

                            if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null)
                                throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null");

                            singleTopology.VisitPrefixFirst(this);
                            if (_visitedPropertiesTotal != 1) throw new ApplicationException("_visitedPropertiesTotal != 1");

                            CollectCreateSortingKeyChain(singleTopology);

                            _replaceQueriedSetName = replaceQueriedSetName;
                        }

                        internal override Func<IQueryable<T>, IOrderedQueryable<T>> CompileMultiEntitySortStrategy()
                            => throw new NotSupportedException(nameof(SingleEFProperty));

                        protected override void CollectSelectValuesExpressionVisit<TProperty>(
                            SortingKeyTopology.ILastProperty property)
                        {
                            if (property == null) throw new ArgumentNullException(nameof(property));
                            if (property.PrefixKeysCount != 0) throw new ApplicationException("property.PrefixKeysCount != 0");
                            if (_visitedPropertiesTotal != 0) throw new ApplicationException("_visitedPropertiesTotal != 0");
                            if (_selectValuesExpression != null) throw new ApplicationException("_selectValuesExpression != null");
                            _selectValuesExpression = EFPropertyExpressionsCache.EFProperty<TProperty>.EntityParameter(
                                EFPropertyExpressionsCache.FromParameter<T>.Instance,
                                property.PropertyName);
                        }

                        protected override Expression GetSelectedValue(SortingKeyTopology.ILastProperty property)
                            => _selectValuesExpression ?? throw new ApplicationException("_selectValuesExpression == null");

                        protected override void ConfigureCreatedSortingKeyQueryOptions(SortingKey sortingKey)
                        {
                            if (sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null)
                                throw new ApplicationException("sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null");
                            sortingKey.ReplaceQueriedSetName = _replaceQueriedSetName ?? sortingKey.ReplaceQueriedSetName;
                        }
                    }
                }
            }
        }
    }
}