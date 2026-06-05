using System;
using System.Linq.Expressions;
using YourCompany.Configuration.EFCore.ExpressionsCaching;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching
{
    internal static partial class SortingKeyQueriesCache
    {
        internal static partial class ForQuery<T>
        {
            private sealed partial class ForTopology
            {
                private abstract partial class TopologyVisitor : SortingKeyTopology.IPrefixFirstPropertiesVisitor
                {
                    private readonly int _singleTopologyPropertiesCount;

                    private Expression _selectValuesExpression;
                    private Expression _createSortingKeyChain;
                    private int _visitedPropertiesTotal;

                    public ICollationAwareModelProvider ModelProvider { get; set; }

                    private TopologyVisitor(SortingKeyTopology.ILastProperty singleTopology)
                    {
                        if (singleTopology == null) throw new ArgumentNullException(nameof(singleTopology));
                        _singleTopologyPropertiesCount = singleTopology.PrefixKeysCount + 1;
                    }

                    internal LambdaExpression CreateSelectValues()
                        => Expression
                            .Lambda(
                                _selectValuesExpression ?? throw new ApplicationException("_selectValuesExpression == null"),
                                EFPropertyExpressionsCache.FromParameter<T>.Instance);

                    internal Delegate CompileSelectKey()
                        => Expression
                            .Lambda(
                                _createSortingKeyChain ?? throw new ApplicationException("_createSortingKeyChain == null"),
                                EFPropertyExpressionsCache.FromParameter<T>.Instance)
                            .Compile();

                    void SortingKeyTopology.IPrefixFirstPropertiesVisitor.VisitProperty<TValue>(
                        SortingKeyTopology.ILastProperty topology)
                    {
                        if (CheckToSKipProperty<TValue>(topology)) return;

                        if (_selectValuesExpression == null)
                        {
                            CollectSelectValuesExpressionVisit<TValue>(topology);
                        }
                        else
                        {
                            CollectCreateSortingKeyChainVisit(topology,
                                () => CreateSortingKey<TValue>(null, default, null, default, false, null));
                        }

                        _visitedPropertiesTotal++;
                    }

                    void SortingKeyTopology.IPrefixFirstPropertiesVisitor.VisitProperty<TValue, TModelValue>(
                        SortingKeyTopology.ILastProperty topology)
                    {
                        if (CheckToSKipProperty<TModelValue>(topology)) return;

                        if (_selectValuesExpression == null)
                        {
                            CollectSelectValuesExpressionVisit<TModelValue>(topology);
                        }
                        else
                        {
                            CollectCreateSortingKeyChainVisit(topology,
                                () => CreateSortingKey<TValue, TModelValue>(null, default, null, default, false, null));
                        }

                        _visitedPropertiesTotal++;
                    }

                    protected virtual bool CheckToSKipProperty<TProperty>(
                        SortingKeyTopology.ILastProperty property) => false;

                    protected abstract void CollectSelectValuesExpressionVisit<TProperty>(
                        SortingKeyTopology.ILastProperty property);

                    protected abstract Expression GetSelectedValue(SortingKeyTopology.ILastProperty property);
                    protected abstract void ConfigureCreatedSortingKeyQueryOptions(SortingKey sortingKey);

                    private void CollectCreateSortingKeyChain(SortingKeyTopology.ILastProperty singleTopology)
                    {
                        if (singleTopology == null) throw new ArgumentNullException(nameof(singleTopology));
                        if (_selectValuesExpression == null) throw new ApplicationException("_selectValuesExpression == null");
                        if (_createSortingKeyChain != null) throw new ApplicationException("_createSortingKeyChain != null");

                        _visitedPropertiesTotal = 0;
                        singleTopology.VisitPrefixFirst(this);

                        if (_visitedPropertiesTotal != _singleTopologyPropertiesCount)
                            throw new ApplicationException("_visitedPropertiesTotal != _singleTopologyPropertiesCount");

                        if (_selectValuesExpression == null) throw new ApplicationException("_selectValuesExpression == null");
                        if (_createSortingKeyChain == null) throw new ApplicationException("_createSortingKeyChain == null");
                    }

                    private void CollectCreateSortingKeyChainVisit(
                        SortingKeyTopology.ILastProperty property, Expression<Action> method)
                    {
                        if (property == null) throw new ArgumentNullException(nameof(property));
                        if (_selectValuesExpression == null) throw new ApplicationException("_selectValuesExpression == null");
                        _createSortingKeyChain = Expression.Call(GetMemberHelper.GetMethodInfo(method),
                            Expression.Constant(ModelProvider),
                            Expression.Constant(property.PropertyOwner),
                            Expression.Constant(property.PropertyName),
                            GetSelectedValue(property),
                            Expression.Constant(property.Descending),
                            _createSortingKeyChain);
                    }

                    private SortingKey CreateSortingKey<TValue>(
                        ICollationAwareModelProvider modelProvider,
                        SortingKeyTopology.PropertiesOwner propertyOwner,
                        string propertyName,
                        TValue value,
                        bool descending,
                        SortingKey prefix)
                        where TValue : IEquatable<TValue>, IComparable<TValue>
                    {
                        var key = new SortingKey.ValueHolding<TValue>(modelProvider, prefix);
                        key.SetValue(propertyOwner, propertyName, value, descending);
                        key.EnsureNoUnexpectedQueryOptionsAfterCreation();

                        if (key.PrefixKeysCount < _singleTopologyPropertiesCount - 1)
                        {
                            key.ReplaceQueriedSetName = null;
                        }
                        else
                        {
                            ConfigureCreatedSortingKeyQueryOptions(key);
                        }

                        return key;
                    }

                    private SortingKey CreateSortingKey<TValue, TModelValue>(
                        ICollationAwareModelProvider modelProvider,
                        SortingKeyTopology.PropertiesOwner propertyOwner,
                        string propertyName,
                        TModelValue value,
                        bool descending,
                        SortingKey prefix)
                        where TValue : IEquatable<TValue>, IComparable<TValue>
                    {
                        var key = new SortingKey.ValueHolding<TValue>.ConvertedFrom<TModelValue>(modelProvider, prefix);
                        key.SetValue(propertyOwner, propertyName, value, descending);
                        key.EnsureNoUnexpectedQueryOptionsAfterCreation();

                        if (key.PrefixKeysCount < _singleTopologyPropertiesCount - 1)
                        {
                            key.ReplaceQueriedSetName = null;
                        }
                        else
                        {
                            ConfigureCreatedSortingKeyQueryOptions(key);
                        }

                        return key;
                    }
                }
            }
        }
    }
}