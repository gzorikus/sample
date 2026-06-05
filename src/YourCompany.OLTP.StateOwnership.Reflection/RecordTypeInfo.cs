using System;
using System.Collections.Generic;
using System.Reflection;

namespace YourCompany.OLTP.StateOwnership.Reflection
{
    public sealed class RecordTypeInfo
    {
        private readonly IDictionary<Type, RecordTypeInfo> _types;
        private readonly RecordConstructionException _recordConstructionLimitationReason;

        public RecordTypesMap Map { get; }
        public Type Type { get; }
        public ConstructorInfo ConstructorInfo { get; }
        public Type RecordDataType { get; private set; }
        public IReadOnlyList<ParameterInfo> RestParameters { get; }
        public bool IsRecord => RecordDataType != null;

        internal RecordTypeInfo(
            RecordTypesMap map,
            Type possiblyRecordType,
            IDictionary<Type, RecordTypeInfo> types,
            Func<RecordTypeInfo, RecordConstructionException> getExtraRecordConstructionLimitationReason)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Type = possiblyRecordType ?? throw new ArgumentNullException(nameof(possiblyRecordType));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _types.Add(possiblyRecordType, this);

            try
            {
                ConstructorInfo = RecordConstructionHelper.TryGetRecordSingleConstructor(possiblyRecordType);
            }
            catch (RecordConstructionException ex)
            {
                _recordConstructionLimitationReason = ex;
                ConstructorInfo = ex.ConstructorInfo;
            }

            if (ConstructorInfo != null)
            {
                var constructorParameters = ConstructorInfo.GetParameters();
                var restParameters = new List<ParameterInfo>(constructorParameters.Length - 1);

                RecordConstructionHelper.VisitRecordConstructorParameters(constructorParameters, stateAccessParameter =>
                {
                    RecordDataType = RecordDataType ?? RecordConstructionHelper.GetRecordDataType(stateAccessParameter);
                }, restParametersVisitor: restParameters.Add);

                RestParameters = restParameters;

                _recordConstructionLimitationReason = _recordConstructionLimitationReason ?? getExtraRecordConstructionLimitationReason?.Invoke(this);
            }
        }

        public bool CheckRecordHasConstructionLimitationReason(Type recordConstructionExceptionType)
        {
            if (recordConstructionExceptionType == null) throw new ArgumentNullException(nameof(recordConstructionExceptionType));
            if (!IsRecord) throw new ApplicationException("!IsRecord");
            if (!typeof(RecordConstructionException).IsAssignableFrom(recordConstructionExceptionType))
                throw new ApplicationException("!typeof(RecordConstructionException).IsAssignableFrom(recordConstructionExceptionType)");

            if (_recordConstructionLimitationReason == null) return false;
            return recordConstructionExceptionType.IsAssignableFrom(_recordConstructionLimitationReason.GetType());
        }

        public RecordConstructionException GetRecordConstructionExceptionForThrowing()
        {
            if (!IsRecord) throw new ApplicationException("!IsRecord");
            if (_recordConstructionLimitationReason == null) throw new ApplicationException("_recordConstructionLimitationReason == null");
            return _recordConstructionLimitationReason.CloneForThrowing() ?? throw new ApplicationException("clonedException == null");
        }
    }
}