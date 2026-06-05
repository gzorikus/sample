using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        internal void SetExplicitLastModifiedAtPropertyIfUnset<TRecordData, TQueryableRecordData>(
            EntityEntry<TQueryableRecordData> entry)
            where TRecordData : class
            where TQueryableRecordData : class, TRecordData
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            var queryableRecordDataType = GetQueryableRecordDataType(typeof(TRecordData));
            if (queryableRecordDataType != typeof(TQueryableRecordData)) throw new ApplicationException("queryableRecordDataType != typeof(TQueryableRecordData)");
            var lastModifiedAtProperty = GetLastModifiedAtProperty<TRecordData, TQueryableRecordData>(entry.Metadata);
            if (!lastModifiedAtProperty.IsShadowProperty())
                SetExplicitLastModifiedAtPropertyIfUnset<TRecordData, TQueryableRecordData>(entry, lastModifiedAtProperty);
        }

        internal virtual void SetExplicitLastModifiedAtPropertyIfUnset<TRecordData, TQueryableRecordData>(
            EntityEntry<TQueryableRecordData> entry, IReadOnlyProperty lastModifiedAtProperty)
            where TRecordData : class
            where TQueryableRecordData : class, TRecordData
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (entry.Context != this) throw new ApplicationException("entry.Context != this");
            if (lastModifiedAtProperty == null) throw new ArgumentNullException(nameof(lastModifiedAtProperty));
            EnsureLocked();

            var queryableRecordDataType = GetQueryableRecordDataType(typeof(TRecordData));
            if (queryableRecordDataType != typeof(TQueryableRecordData)) throw new ApplicationException("queryableRecordDataType != typeof(TQueryableRecordData)");

            if (lastModifiedAtProperty.ClrType == typeof(DateTime))
            {
                var property = entry.Property<DateTime>(lastModifiedAtProperty.Name);
                if (property.CurrentValue == default) property.CurrentValue = ConfiguredUtcNow;
            }
            else if (lastModifiedAtProperty.ClrType == typeof(DateTimeOffset))
            {
                var property = entry.Property<DateTimeOffset>(lastModifiedAtProperty.Name);
                if (property.CurrentValue == default) property.CurrentValue = ConfiguredUtcNow;
            }
            else
            {
                throw new ApplicationException("entry.ClrType != typeof(DateTime) && entry.ClrType != typeof(DateTimeOffset)");
            }
        }

        protected virtual void DeclareLastModifiedAtProperty(
            ModelBuilder modelBuilder, Type recordDataType, Type queryableRecordDataType)
        {
            var entityType = modelBuilder.Entity(queryableRecordDataType);
            string lastModifiedAtPropertyName = GetLastModifiedAtPropertyName(recordDataType);

            var lastModifiedAtProperty = entityType.Metadata.FindProperty(lastModifiedAtPropertyName);
            if (lastModifiedAtProperty == null)
            {
                string lastModifiedAtColumnName = GetLastModifiedAtColumnName(recordDataType);
                entityType.Property<DateTime>(lastModifiedAtPropertyName).HasColumnName(lastModifiedAtColumnName);
            }
            else
            {
                if (lastModifiedAtProperty.ClrType != typeof(DateTime)
                    && lastModifiedAtProperty.ClrType != typeof(DateTimeOffset))
                    throw new ApplicationException("lastModifiedAtProperty.ClrType != typeof(DateTime) && lastModifiedAtProperty.ClrType != typeof(DateTimeOffset)");
            }
        }

        internal IReadOnlyProperty GetLastModifiedAtProperty<TRecordData, TQueryableRecordData>(
            IReadOnlyEntityType entityType)
            where TRecordData : class
            where TQueryableRecordData : class, TRecordData
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            string lastModifiedAtPropertyName = GetLastModifiedAtPropertyName(typeof(TRecordData));
            return entityType.FindProperty(lastModifiedAtPropertyName) ?? throw new ApplicationException("lastModifiedAtProperty == null");
        }

        internal virtual string GetLastModifiedAtPropertyName(Type recordDataType)
        {
            if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));
            _ = GetQueryableRecordDataType(recordDataType);
            string lastModifiedAtPropertyName
                = Configuration.ByRecordDataTypeFullName?.TryGetValue(recordDataType.FullName, out var by) == true
                    ? by.LastModifiedAtPropertyName ?? Configuration.DefaultLastModifiedAtPropertyName
                    : Configuration.DefaultLastModifiedAtPropertyName;
            return lastModifiedAtPropertyName ?? throw new ApplicationException("lastModifiedAtPropertyName == null");
        }

        internal virtual string GetLastModifiedAtColumnName(Type recordDataType)
        {
            if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));
            _ = GetQueryableRecordDataType(recordDataType);
            string lastModifiedAtColumnName
                = Configuration.ByRecordDataTypeFullName?.TryGetValue(recordDataType.FullName, out var by) == true
                    ? by.LastModifiedAtColumnName ?? Configuration.DefaultLastModifiedAtColumnName
                    : Configuration.DefaultLastModifiedAtColumnName;
            return lastModifiedAtColumnName ?? throw new ApplicationException("lastModifiedAtColumnName == null");
        }
    }
}