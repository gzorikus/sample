using System;
using System.Collections.Generic;
using System.Reflection;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.Reflection.Composition;
using RecordConstructionHelper = YourCompany.OLTP.StateOwnership.Reflection.RecordConstructionHelper;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection
{
    public abstract class ComposableRecordTypesProvider : ComposableTypesProvider
    {
        public sealed override IReadOnlyList<ParameterInfo> TryGetComposableConstructorParameters(
            ComposableTypeInfo composableTypeInfo, Action<CompositionException> constructionLimitationReasonsCollector)
        {
            if (composableTypeInfo == null) throw new ArgumentNullException(nameof(composableTypeInfo));
            if (constructionLimitationReasonsCollector == null) throw new ArgumentNullException(nameof(constructionLimitationReasonsCollector));

            ConstructorInfo recordConstructorInfo = null;

            try
            {
                recordConstructorInfo = RecordConstructionHelper.TryGetRecordSingleConstructor(composableTypeInfo.Type);
            }
            catch (RecordConstructionException ex)
            {
                constructionLimitationReasonsCollector(
                    new RecordCompositionException.HasRecordConstructionLimitations(composableTypeInfo, ex));
                recordConstructorInfo = ex.ConstructorInfo;
            }

            List<ParameterInfo> recordConstructorParameters = null;

            if (recordConstructorInfo != null)
            {
                var constructorParameters = recordConstructorInfo.GetParameters();
                recordConstructorParameters = new List<ParameterInfo>(constructorParameters.Length - 1);

                RecordConstructionHelper.VisitRecordConstructorParameters(
                    constructorParameters,
                    stateAccessParameterVisitor: null,
                    restParametersVisitor: recordConstructorParameters.Add);
            }

            return recordConstructorParameters;
        }

        public sealed override ConstructorInfo TryGetSingleComposableConstructor(
            ComposableTypeInfo composableTypeInfo, IReadOnlyList<ParameterInfo> composableConstructorParameters)
        {
            if (composableTypeInfo == null) throw new ArgumentNullException(nameof(composableTypeInfo));
            if (composableConstructorParameters == null) throw new ArgumentNullException(nameof(composableConstructorParameters));

            ConstructorInfo singleConstructorInfo = null;

            for (int i = 0; i < composableConstructorParameters.Count; i++)
            {
                var constructorInfo = (ConstructorInfo)composableConstructorParameters[i].Member;
                if (singleConstructorInfo == null)
                {
                    singleConstructorInfo = constructorInfo;
                }
                else
                {
                    if (constructorInfo != singleConstructorInfo) throw new ApplicationException("constructorInfo != singleParametersConstructorInfo");
                }
            }

            if (singleConstructorInfo != null)
            {
                if (singleConstructorInfo.DeclaringType != composableTypeInfo.Type)
                    throw new ApplicationException("singleParametersConstructorInfo.DeclaringType != composableTypeInfo.Type");
            }
            else
            {
                singleConstructorInfo = RecordConstructionHelper.GetSingleRecordConstructor(composableTypeInfo.Type);
            }

            return singleConstructorInfo;
        }
    }
}