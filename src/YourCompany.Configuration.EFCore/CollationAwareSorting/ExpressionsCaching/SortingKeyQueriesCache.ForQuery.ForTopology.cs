using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
                private readonly TopologyCacheKey _cacheKey;
                private readonly LambdaExpression _selectValues;
                private readonly Delegate _selectKey;
                private readonly Func<IQueryable<T>, IOrderedQueryable<T>> _multiEntitySort;
                private readonly Func<IQueryable<T>, CancellationToken, Task<List<SortingKey>>> _getKeysFrom;
                private readonly ConcurrentDictionary<Type, Delegate> _getKeysFromByExtraValueType;
                private readonly Func<Type, Delegate> _createGetKeysFromByExtraValueTypeItemNonExclusiveStrategy;

                internal ForTopology(TopologyCacheKey cacheKey)
                {
                    _cacheKey = cacheKey;

                    var singleTopology = cacheKey.SingleTopology ?? throw new ApplicationException("singleTopology == null");
                    string singleEntityReplacingQueriedSetName = cacheKey.SingleEntityReplacingQueriedSetName;
                    var multiEntityValueTuplePropertyOwnerIndecies = cacheKey.MultiEntityValueTuplePropertyOwnerIndecies;

                    TopologyVisitor topologyVisitor;

                    if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null)
                    {
                        if (multiEntityValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("multiEntityValueTuplePropertyOwnerIndecies");
                        singleTopology.EnsureCompatibleWithSingleEntityQueries(
                            typeof(T), singleEntityReplacingQueriedSet: singleEntityReplacingQueriedSetName != null);

                        topologyVisitor = singleTopology.PrefixKeysCount == 0
                            ? new TopologyVisitor.SingleEFProperty(singleTopology, singleEntityReplacingQueriedSetName)
                            : new TopologyVisitor.MultiEFProperties.ForEntity(singleTopology, singleEntityReplacingQueriedSetName);
                    }
                    else
                    {
                        if (singleEntityReplacingQueriedSetName != null) throw new ApplicationException("singleEntityReplacingQueriedSetName");
                        if (multiEntityValueTuplePropertyOwnerIndecies == null) throw new ApplicationException("multiEntityValueTuplePropertyOwnerIndecies == null");
                        if (multiEntityValueTuplePropertyOwnerIndecies.Count == 0) throw new ApplicationException("multiEntityValueTuplePropertyOwnerIndecies.Count == 0");
                        topologyVisitor = new TopologyVisitor.MultiEFProperties.ForEntitiesValueTuple(
                            singleTopology, multiEntityValueTuplePropertyOwnerIndecies);
                    }

                    _selectValues = topologyVisitor.CreateSelectValues();
                    _selectKey = topologyVisitor.CompileSelectKey();

                    if (multiEntityValueTuplePropertyOwnerIndecies != null)
                        _multiEntitySort = topologyVisitor.CompileMultiEntitySortStrategy();

                    _getKeysFrom = CompileGetKeysFrom();
                    _getKeysFromByExtraValueType = new ConcurrentDictionary<Type, Delegate>();
                    _createGetKeysFromByExtraValueTypeItemNonExclusiveStrategy = CompileGetKeysFromWithExtraValue;
                }

                internal Task<List<SortingKey>> GetKeysFrom(IQueryable<T> queryable, CancellationToken cancellationToken)
                    => _getKeysFrom.Invoke(queryable, cancellationToken);

                internal Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeysFrom<TExtraValue>(
                    IQueryable<T> queryable,
                    Expression<Func<T, TExtraValue>> extraValueSelector,
                    CancellationToken cancellationToken)
                {
                    var getKeysFrom = _getKeysFromByExtraValueType.GetOrAdd(
                        typeof(TExtraValue), _createGetKeysFromByExtraValueTypeItemNonExclusiveStrategy);

                    if (getKeysFrom is not Func<
                        IQueryable<T>,
                        Expression<Func<T, TExtraValue>>,
                        CancellationToken,
                        Task<List<SortingKeyExtraValuePair<TExtraValue>>>> matchingDelegate)
                        throw new ApplicationException("matchingDelegate == null");

                    return matchingDelegate(queryable, extraValueSelector, cancellationToken);
                }

                internal IOrderedQueryable<T> MultiEntitySort(IQueryable<T> queryable)
                {
                    bool shouldSort = _cacheKey.SingleTopology != null;
                    bool canSort = _multiEntitySort != null;
                    if (shouldSort != canSort) throw new ApplicationException("shouldSort != canSort");
                    return _multiEntitySort?.Invoke(queryable) ?? throw new ApplicationException("_multiEntitySort == null");
                }

                private Func<IQueryable<T>, CancellationToken, Task<List<SortingKey>>> CompileGetKeysFrom()
                {
                    if (_selectValues == null) throw new ApplicationException("_selectValues == null");
                    Expression<Func<Task>> method = () => GetKeysFromPrototype<int>(null, default);
                    var queryable = Expression.Parameter(typeof(IQueryable<T>), "queryable");
                    var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    var call = Expression.Call(
                        Expression.Constant(this),
                        GetMemberHelper.GetMethodInfo(method).GetGenericMethodDefinition().MakeGenericMethod(
                            new[] { _selectValues.ReturnType }),
                        queryable,
                        cancellationToken);
                    return Expression
                        .Lambda<Func<IQueryable<T>, CancellationToken, Task<List<SortingKey>>>>(
                            call, queryable, cancellationToken)
                        .Compile();
                }

                private async Task<List<SortingKey>> GetKeysFromPrototype<TValues>(
                    IQueryable<T> queryable, CancellationToken cancellationToken)
                {
                    var selectValues = _selectValues as Expression<Func<T, TValues>> ?? throw new ApplicationException("selectValues == null");
                    var selectKey = _selectKey as Func<TValues, SortingKey> ?? throw new ApplicationException("selectKey == null");

                    var keys = new List<SortingKey>();

                    var asyncEnumerable = queryable.Select(selectValues).AsAsyncEnumerable().WithCancellation(cancellationToken);
                    await foreach (var value in asyncEnumerable)
                        keys.Add(selectKey(value));

                    return keys;
                }

                private Delegate CompileGetKeysFromWithExtraValue(Type extraValueType)
                {
                    if (_selectValues == null) throw new ApplicationException("_selectValues == null");
                    if (extraValueType == null) throw new ArgumentNullException(nameof(extraValueType));
                    Expression<Func<Task>> method = () => GetKeysFromPrototype<int, int>(null, null, default);
                    var queryable = Expression.Parameter(typeof(IQueryable<T>), "queryable");
                    var extraValueSelector = Expression.Parameter(
                        typeof(Expression<>).MakeGenericType(
                            new[] { typeof(Func<,>).MakeGenericType(
                                new[] { typeof(T), extraValueType }) }),
                        "extraValueSelector");
                    var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    var call = Expression.Call(
                        Expression.Constant(this),
                        GetMemberHelper.GetMethodInfo(method).GetGenericMethodDefinition().MakeGenericMethod(
                            new[] { _selectValues.ReturnType, extraValueType }),
                        queryable,
                        extraValueSelector,
                        cancellationToken);
                    return Expression.Lambda(call, queryable, extraValueSelector, cancellationToken).Compile();
                }

                private async Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeysFromPrototype<TValues, TExtraValue>(
                    IQueryable<T> queryable,
                    Expression<Func<T, TExtraValue>> extraValueSelector,
                    CancellationToken cancellationToken)
                {
                    var selectValues = _selectValues as Expression<Func<T, TValues>> ?? throw new ApplicationException("selectValues == null");
                    var selectKey = _selectKey as Func<TValues, SortingKey> ?? throw new ApplicationException("selectKey == null");

                    var keys = new List<SortingKeyExtraValuePair<TExtraValue>>();

                    var asyncEnumerable = queryable.Select(ExtraValueSelectCombine.ToTuple(selectValues, extraValueSelector))
                        .AsAsyncEnumerable()
                        .WithCancellation(cancellationToken);
                    await foreach (var tuple in asyncEnumerable)
                    {
                        keys.Add(new SortingKeyExtraValuePair<TExtraValue>
                        {
                            SortingKey = selectKey(tuple.Item1),
                            ExtraValue = tuple.Item2
                        });
                    }

                    return keys;
                }
            }

            private sealed class ExtraValueSelectCombine : ExpressionVisitor
            {
                private readonly ParameterExpression _extraValueParam;

                internal ExtraValueSelectCombine(ParameterExpression extraValueParam)
                    => _extraValueParam = extraValueParam ?? throw new ArgumentNullException(nameof(extraValueParam));

                internal static Expression<Func<T, (TValues, TExtraValue)>> ToTuple<TValues, TExtraValue>(
                    Expression<Func<T, TValues>> selectValues, Expression<Func<T, TExtraValue>> selectExtraValue)
                {
                    if (selectValues.Parameters[0] != EFPropertyExpressionsCache.FromParameter<T>.Instance)
                        throw new ApplicationException("selectValues.Parameters[0] != EFPropertyExpressionsCache.FromParameter<T>.Instance");

                    var parameterReplaceVisitor = new ExtraValueSelectCombine(selectExtraValue.Parameters[0]);
                    return Expression.Lambda<Func<T, (TValues, TExtraValue)>>(
                        ValueTupleHelper.Expressions.CreateValueTupleWithoutRest(
                            selectValues.Body, parameterReplaceVisitor.Visit(selectExtraValue.Body)),
                        EFPropertyExpressionsCache.FromParameter<T>.Instance);
                }

                protected override Expression VisitParameter(ParameterExpression node)
                    => node == _extraValueParam ? EFPropertyExpressionsCache.FromParameter<T>.Instance : base.VisitParameter(node);
            }
        }
    }
}