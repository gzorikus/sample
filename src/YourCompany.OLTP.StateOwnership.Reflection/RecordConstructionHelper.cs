using System;
using System.Collections.Generic;
using System.Reflection;

namespace YourCompany.OLTP.StateOwnership.Reflection
{
    public static class RecordConstructionHelper
    {
        public static Type GetRecordDataType(ParameterInfo stateAccessParameter)
        {
            if (stateAccessParameter == null) throw new ArgumentNullException(nameof(stateAccessParameter));

            if (!stateAccessParameter.ParameterType.IsConstructedGenericType)
                throw new ApplicationException("!stateAccessParameter.ParameterType.IsConstructedGenericType");

            if (stateAccessParameter.ParameterType.GetGenericTypeDefinition() != typeof(IStateAccess<>))
                throw new ApplicationException("stateAccessParameter.ParameterType.GetGenericTypeDefinition() != typeof(IStateAccess<>)");

            return stateAccessParameter.ParameterType.GetGenericArguments()[0];
        }

        public static ConstructorInfo GetSingleRecordConstructor(Type recordType)
        {
            if (recordType == null) throw new ArgumentNullException(nameof(recordType));
            return TryGetRecordSingleConstructor(recordType) ?? throw new ApplicationException("constructorInfo == null");
        }

        public static ConstructorInfo TryGetRecordSingleConstructor(Type recordType)
        {
            if (recordType == null) throw new ArgumentNullException(nameof(recordType));

            ConstructorInfo singleRecordConstructor = null;

            var constructors = GetAllInstanceConstructors(recordType);
            for (int constructorIndex = 0; constructorIndex < constructors.Length; constructorIndex++)
            {
                var constructor = constructors[constructorIndex];
                int stateAccessParameters = 0;

                VisitRecordConstructorParameters(constructor, stateAccessParameterVisitor: _ =>
                {
                    stateAccessParameters++;

                    if (singleRecordConstructor != null)
                    {
                        if (stateAccessParameters > 1) throw new RecordConstructionException.ConstructorMustHaveSingleStateAccessParameter(constructor);
                        throw new RecordConstructionException.ConstructorMustBeSingle(singleRecordConstructor, constructor);
                    }

                    if (!constructor.IsAssembly) throw new RecordConstructionException.ConstructorMustBeInternal(constructor);
                    singleRecordConstructor = constructor;
                }, restParametersVisitor: null);
            }

            return singleRecordConstructor;
        }

        public static ConstructorInfo[] GetAllInstanceConstructors(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        public static void VisitRecordConstructorParameters(ConstructorInfo constructorInfo, Action<ParameterInfo> stateAccessParameterVisitor, Action<ParameterInfo> restParametersVisitor)
        {
            if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
            VisitRecordConstructorParameters(constructorInfo.GetParameters(), stateAccessParameterVisitor, restParametersVisitor);
        }

        public static void VisitRecordConstructorParameters(IReadOnlyList<ParameterInfo> parameters, Action<ParameterInfo> stateAccessParameterVisitor, Action<ParameterInfo> restParametersVisitor)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
            {
                var parameter = parameters[parameterIndex];

                bool stateAccessParameter = false;

                if (parameter.ParameterType.IsGenericType && parameter.ParameterType.IsInterface)
                {
                    var genericInterfaceDefinition = parameter.ParameterType.GetGenericTypeDefinition();
                    stateAccessParameter = genericInterfaceDefinition == typeof(IStateAccess<>);
                }

                if (stateAccessParameter)
                {
                    stateAccessParameterVisitor?.Invoke(parameter);
                }
                else
                {
                    restParametersVisitor?.Invoke(parameter);
                }
            }
        }
    }
}