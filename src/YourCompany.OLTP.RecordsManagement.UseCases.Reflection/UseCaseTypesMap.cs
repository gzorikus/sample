using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCompany.OLTP.RecordsManagement.UseCases.Reflection
{
    public sealed class UseCaseTypesMap : IReadOnlyDictionary<Type, UseCaseTypeInfo>
    {
        private readonly Dictionary<Type, UseCaseTypeInfo> _types;

        private readonly HashSet<Type> _nonGenericUseCaseTypes = new HashSet<Type>
        {
            typeof(RecordsBatchTransactionCallback.IExtraInterfacesProvider),
            typeof(RecordsBatchTransactionUseCases.IChangesPreparer),
            typeof(RecordsBatchTransactionUseCases.IFinishingHandler)
        };

        private readonly HashSet<Type> _singleArgumentContravariantUseCaseGenericTypeDefinitions = new HashSet<Type>
        {
            typeof(RecordsBatchTransactionUseCases.IAuthorizer<>),
            typeof(RecordsBatchTransactionUseCases.IChangesPreparer<>),
            typeof(RecordsBatchTransactionUseCases.IFinishingHandler<>)
        };

        public IEnumerable<Type> Keys => _types.Keys;
        public IEnumerable<UseCaseTypeInfo> Values => _types.Values;
        public int Count => _types.Count;
        public UseCaseTypeInfo this[Type key] => _types[key];

        public UseCaseTypesMap(
            IEnumerable<Type> typesToMap,
            IEnumerable<Type> extraNonGenericUseCaseTypes = null,
            IEnumerable<Type> extraSingleArgumentContravariantUseCaseGenericTypeDefinitions = null)
        {
            _types = new Dictionary<Type, UseCaseTypeInfo>();

            if (extraNonGenericUseCaseTypes != null)
            {
                foreach (var nonGenericUseCaseType in extraNonGenericUseCaseTypes)
                {
                    if (nonGenericUseCaseType.IsGenericType) throw new ApplicationException("nonGenericUseCaseType.IsGenericType");
                    if (!_nonGenericUseCaseTypes.Add(nonGenericUseCaseType))
                        throw new ApplicationException("!_nonGenericUseCaseTypes.Add(nonGenericUseCaseType)");
                }
            }

            if (extraSingleArgumentContravariantUseCaseGenericTypeDefinitions != null)
            {
                foreach (var genericTypeDefinition in extraSingleArgumentContravariantUseCaseGenericTypeDefinitions)
                {
                    if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ApplicationException("!genericTypeDefinition.IsGenericTypeDefinition");
                    if (genericTypeDefinition.GetGenericArguments().Length != 1) throw new ApplicationException("genericTypeDefinition.GetGenericArguments().Length != 1");
                    if (!_singleArgumentContravariantUseCaseGenericTypeDefinitions.Add(genericTypeDefinition))
                        throw new ApplicationException("!_singleArgumentContravariantUseCaseGenericTypeDefinitions.Add(genericTypeDefinition)");
                }
            }

            Populate(typesToMap);
        }

        public bool ContainsKey(Type key) => _types.ContainsKey(key);
        public bool TryGetValue(Type key, out UseCaseTypeInfo value) => _types.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<Type, UseCaseTypeInfo>> GetEnumerator() => _types.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();

        public bool ExpectsNonGenericUseCaseType(Type nonGenericUseCaseType)
        {
            if (nonGenericUseCaseType == null) throw new ArgumentNullException(nameof(nonGenericUseCaseType));
            if (nonGenericUseCaseType.IsGenericType) throw new ApplicationException("nonGenericUseCaseType.IsGenericType");
            return _nonGenericUseCaseTypes.Contains(nonGenericUseCaseType);
        }

        public bool ExpectsSingleArgumentContravariantUseCaseGenericTypeDefinition(Type genericTypeDefinition)
        {
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
            if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ApplicationException("!genericTypeDefinition.IsGenericTypeDefinition");
            return _singleArgumentContravariantUseCaseGenericTypeDefinitions.Contains(genericTypeDefinition);
        }

        private void Populate(IEnumerable<Type> typesToMap)
        {
            if (typesToMap == null) throw new ArgumentNullException(nameof(typesToMap));
            foreach (var possiblyUseCasesImplementorType in typesToMap)
                _ = new UseCaseTypeInfo(this, possiblyUseCasesImplementorType, _types);
        }
    }
}