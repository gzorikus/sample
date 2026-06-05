using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCompany.Reflection.Composition
{
    public sealed partial class ComposableTypeInfo : IReadOnlyDictionary<Type, ComposableTypeInfo>
    {
        private static readonly IReadOnlyDictionary<Type, ComposableTypeInfo> NoDependencies
            = new Dictionary<Type, ComposableTypeInfo>();

        private Dictionary<Type, ComposableTypeInfo> _dependencies;
        private int? _visitingComposableConstructorParameterIndex;
        private ComposableTypeInfo _visitingComposableConstructorParameterDependency;

        public IEnumerable<Type> Keys => GetDependenciesReadonly().Keys;
        public IEnumerable<ComposableTypeInfo> Values => GetDependenciesReadonly().Values;
        public int Count => GetDependenciesReadonly().Count;
        public ComposableTypeInfo this[Type key] => GetDependenciesReadonly()[key];

        public bool ContainsKey(Type key) => GetDependenciesReadonly().ContainsKey(key);

        public bool TryGetValue(Type key, out ComposableTypeInfo value)
            => GetDependenciesReadonly().TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<Type, ComposableTypeInfo>> GetEnumerator()
            => GetDependenciesReadonly().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetDependenciesReadonly().GetEnumerator();

        private void TryCollectComposableConstructorDependency(Type parameterType)
        {
            if (parameterType == null) throw new ArgumentNullException(nameof(parameterType));
            var dependencyType = TryExtractDependencyTypeFromEnumerable(parameterType);
            if (_types.TryGetValue(dependencyType, out ComposableTypeInfo dependencyTypeInfo))
                AddDependency(dependencyType, dependencyTypeInfo);
        }

        private void VisitComposableDependencyComposables(Action<ComposableTypeInfo> dependencyComposableVisitor)
            => VisitComposableConstructorParameterDependencies(
                dependencyTypeInfo => dependencyTypeInfo.VisitDependencyComposables(dependencyComposableVisitor));

        private void VisitComposableConstructorParameterDependencies(Action<ComposableTypeInfo> dependencyVisitor)
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (_dependencies == null) return;
            VisitComposableConstructorParameters(parameterType =>
            {
                var dependencyType = TryExtractDependencyTypeFromEnumerable(parameterType);
                if (_dependencies.TryGetValue(dependencyType, out ComposableTypeInfo dependencyTypeInfo))
                {
                    var prevDependencyTypeInfo = _visitingComposableConstructorParameterDependency;
                    _visitingComposableConstructorParameterDependency = dependencyTypeInfo;
                    try
                    {
                        dependencyVisitor(dependencyTypeInfo);
                    }
                    finally
                    {
                        _visitingComposableConstructorParameterDependency = prevDependencyTypeInfo;
                    }
                }
            });
        }

        private void VisitComposableConstructorParameters(Action<Type> parameterVisitor)
        {
            if (!IsComposable) throw new ApplicationException("!IsComposable");
            if (CheckComposableHasConstructionLimitationReason(typeof(CompositionException.DependenciesLoopDetected)))
                throw new ApplicationException("CheckComposableHasConstructionLimitationReason(typeof(CompositionException.DependenciesLoopDetected))");

            var parameters = _composableConstructorParameters ?? throw new ApplicationException("parameters == null");
            for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
            {
                var parameter = parameters[parameterIndex];
                int? prevParameterIndex = _visitingComposableConstructorParameterIndex;
                if (prevParameterIndex.HasValue)
                {
                    AddConstructionLimitationReason(
                        new CompositionException.DependenciesLoopDetected(this, prevParameterIndex.Value));
                    break;
                }

                _visitingComposableConstructorParameterIndex = parameterIndex;
                try
                {
                    parameterVisitor(parameter.ParameterType);

                    if (CheckComposableHasConstructionLimitationReason(typeof(CompositionException.DependenciesLoopDetected)))
                        break;
                }
                finally
                {
                    _visitingComposableConstructorParameterIndex = prevParameterIndex;
                }
            }
        }

        private void VisitDependencyComposables(Action<ComposableTypeInfo> dependencyComposableVisitor)
        {
            if (IsComposable)
            {
                dependencyComposableVisitor(this);
            }
            else
            {
                if (!CheckIsComposableAbstraction())
                    throw new ApplicationException("!CheckIsComposableAbstraction()");

                if (_dependencies != null)
                {
                    foreach (var dependencyComposableTypeInfo in _dependencies.Values)
                    {
                        if (!dependencyComposableTypeInfo.IsComposable) throw new ApplicationException("!dependencyComposableTypeInfo.IsComposable");
                        dependencyComposableVisitor(dependencyComposableTypeInfo);
                    }
                }
            }
        }

        private IReadOnlyDictionary<Type, ComposableTypeInfo> GetDependenciesReadonly() => _dependencies ?? NoDependencies;

        private void AddDependency(Type dependencyType, ComposableTypeInfo dependencyTypeInfo)
        {
            _dependencies = _dependencies ?? new Dictionary<Type, ComposableTypeInfo>();
            _dependencies.Add(dependencyType, dependencyTypeInfo);
        }

        private static Type TryExtractDependencyTypeFromEnumerable(Type parameterType)
        {
            if (!parameterType.IsConstructedGenericType)
                return parameterType;

            return parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? parameterType.GetGenericArguments()[0]
                : parameterType;
        }
    }
}