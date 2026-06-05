using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.OLTP.StateOwnership.Reflection.EFCore
{
    public static partial class MetadataHelper
    {
        public static IReadOnlyDictionary<Type, Type> CollectRecordDataTypeImplementations(
            this RecordTypesMap recordTypesMap, string reserveObjectNamesPrefix, IReadOnlyModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var recordDataTypeImplementations = new Dictionary<Type, Type>();

            var providedRecordDataTypes = recordTypesMap.Values
                .Where(domainTypeInfo => domainTypeInfo.IsRecord)
                .Select(recordInfo => recordInfo.RecordDataType).ToHashSet();

            foreach (IReadOnlyEntityType entityType in model.GetEntityTypes())
            {
                var implementedRecordTypes = entityType.ClrType.FindInterfaces(
                    (implementedType, _) => providedRecordDataTypes.Contains(implementedType), null);

                if (implementedRecordTypes.Length > 0)
                {
                    if (entityType.GetTableName() == null) throw new ApplicationException("entityType.GetTableName() == null");
                    if (!string.IsNullOrEmpty(reserveObjectNamesPrefix)
                        && entityType.GetTableName().StartsWith(reserveObjectNamesPrefix))
                        throw new ApplicationException("entityType.GetTableName().StartsWith(reserveObjectNamesPrefix)");
                }

                for (int i = 0; i < implementedRecordTypes.Length; i++)
                    recordDataTypeImplementations.Add(implementedRecordTypes[i], entityType.ClrType);
            }

            if (providedRecordDataTypes.Count != recordDataTypeImplementations.Count)
                throw new ApplicationException("providedRecordDataTypes.Count != recordDataTypeImplementations.Count");

            return recordDataTypeImplementations;
        }
    }
}