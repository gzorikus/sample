using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using YourCompany.OLTP.StateOwnership.Reflection.EFCore;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        private static IReadOnlyDictionary<Type, Type> _singleRuntimeModelQueryableRecordDataTypesPerConfigurationType;

        public static IReadOnlyDictionary<Type, Type> SingleRuntimeModelQueryableRecordDataTypesPerConfigurationType
            => _singleRuntimeModelQueryableRecordDataTypesPerConfigurationType ?? throw new ApplicationException("_singleRuntimeModelQueryableRecordDataTypesPerConfigurationType == null");

        public Type GetQueryableRecordDataType(Type recordDataType)
        {
            if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));
            return SingleRuntimeModelQueryableRecordDataTypesPerConfigurationType.GetValueOrDefault(recordDataType)
                ?? throw new ApplicationException("queryableRecordDataType == null");
        }

        protected virtual void CollectQueryableRecordDataTypes(string sanitizedObjectNamesPrefix, ModelBuilder modelBuilder)
        {
            if (sanitizedObjectNamesPrefix == null) throw new ArgumentNullException(nameof(sanitizedObjectNamesPrefix));
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            var queryableRecordDataTypes = CollectQueryableRecordDataTypes(sanitizedObjectNamesPrefix, modelBuilder.Model);

            if (!ConfiguratorsLoadingContext.IsDesignTime)
            {
                _ = (queryableRecordDataTypes.Keys, queryableRecordDataTypes.Values);
                if (Interlocked.CompareExchange(
                    ref _singleRuntimeModelQueryableRecordDataTypesPerConfigurationType, queryableRecordDataTypes, null) != null)
                    throw new ApplicationException("Interlocked.CompareExchange(ref _singleRuntimeModelQueryableRecordDataTypesPerConfigurationType, queryableRecordDataTypes, null) != null");
            }

            foreach (var kvp in queryableRecordDataTypes)
            {
                DeclarePrimaryKey(modelBuilder, recordDataType: kvp.Key, queryableRecordDataType: kvp.Value);
            }
        }

        protected virtual IReadOnlyDictionary<Type, Type> CollectQueryableRecordDataTypes(
            string sanitizedObjectNamesPrefix, IReadOnlyModel model)
        {
            if (sanitizedObjectNamesPrefix == null) throw new ArgumentNullException(nameof(sanitizedObjectNamesPrefix));
            if (model == null) throw new ArgumentNullException(nameof(model));
            return ConfiguratorsLoadingContext.RecordTypesMap.CollectRecordDataTypeImplementations(
                sanitizedObjectNamesPrefix, model);
        }
    }
}