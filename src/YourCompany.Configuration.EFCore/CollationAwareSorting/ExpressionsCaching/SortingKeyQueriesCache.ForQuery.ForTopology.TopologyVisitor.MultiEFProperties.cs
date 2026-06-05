using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using YourCompany.Reflection;

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
                    internal abstract partial class MultiEFProperties : TopologyVisitor
                    {
                        private readonly Expression[] _multiEFPropertyExpressions;
                        private readonly List<MemberExpression> _selectValuesValueTupleProperties;

                        private MultiEFProperties(SortingKeyTopology.ILastProperty singleTopology) : base(singleTopology)
                        {
                            if (_singleTopologyPropertiesCount < 2) throw new ApplicationException("_singleTopologyPropertiesCount < 2");
                            _multiEFPropertyExpressions = new Expression[_singleTopologyPropertiesCount];
                            _selectValuesValueTupleProperties = new List<MemberExpression>(_singleTopologyPropertiesCount);
                        }

                        private void BuildMultiEFPropertySelectValuesExpression()
                        {
                            if (_selectValuesExpression != null) throw new ApplicationException("_selectValuesExpression != null");
                            _selectValuesExpression
                                = ValueTupleHelper.Expressions.CreateValueTupleRestNormalized(_multiEFPropertyExpressions);

                            ValueTupleHelper.Expressions.CollectValueTuplePropertiesRecursive(
                                _selectValuesExpression,
                                _selectValuesValueTupleProperties,
                                restNormalized: true);

                            if (_selectValuesValueTupleProperties.Count != _singleTopologyPropertiesCount)
                                throw new ApplicationException("_selectValuesValueTupleProperties.Count != _singleTopologyPropertiesCount");
                        }

                        protected override Expression GetSelectedValue(SortingKeyTopology.ILastProperty property)
                            => _selectValuesValueTupleProperties[property.PrefixKeysCount]
                                ?? throw new ApplicationException("_selectValuesValueTupleProperties[property.PrefixKeysCount] == null");
                    }
                }
            }
        }
    }
}