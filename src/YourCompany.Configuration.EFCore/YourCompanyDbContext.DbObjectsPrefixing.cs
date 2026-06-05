using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        protected virtual string SanitizeObjectNamesPrefix(string objectNamesPrefix)
        {
            if (string.IsNullOrEmpty(objectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(objectNamesPrefix)");

            string singleSanitizedObjectNamesPrefix = null;

            for (int i = 0; i < Configurators.Count; i++)
            {
                string sanitizedObjectNamesPrefix = SanitizeObjectNamesPrefix(objectNamesPrefix, Configurators[i]);
                if (sanitizedObjectNamesPrefix != null)
                {
                    if (singleSanitizedObjectNamesPrefix != null)
                        throw new ApplicationException("singleSanitizedObjectNamesPrefix != null");
                    singleSanitizedObjectNamesPrefix = sanitizedObjectNamesPrefix;
                }
            }

            return singleSanitizedObjectNamesPrefix ?? throw new ApplicationException("singleSanitizedObjectNamesPrefix");
        }

        protected virtual string SanitizeObjectNamesPrefix(
            string objectNamesPrefix, YourCompanyDbContextConfigurator configurator)
        {
            if (string.IsNullOrEmpty(objectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(objectNamesPrefix)");
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            return configurator.SanitizeObjectNamesPrefix(ConfiguratorsLoadingContext, objectNamesPrefix);
        }

        protected virtual void PrefixDbObjects(string sanitizedObjectNamesPrefix, ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            PrefixTableRelatedObjectNamesBeforeAffectingByTableNamePrefixing(sanitizedObjectNamesPrefix, modelBuilder);
            PrefixTableNamesAfterAllRelatedObjectNamesFixed(sanitizedObjectNamesPrefix, modelBuilder);
        }

        protected virtual void PrefixTableRelatedObjectNamesBeforeAffectingByTableNamePrefixing(string sanitizedObjectNamesPrefix, ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
                if (entityType.GetTableName() != null && !entityType.IsTableExcludedFromMigrations())
                    PrefixTableRelatedObjectsBeforeAffectingByTableNamePrefixing(sanitizedObjectNamesPrefix, entityType);
        }

        protected virtual void PrefixTableRelatedObjectsBeforeAffectingByTableNamePrefixing(string sanitizedObjectNamesPrefix, IMutableEntityType entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (entityType.GetTableName() == null) throw new ApplicationException("entityType.GetTableName() == null");
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            if (entityType.GetTableName().Contains(sanitizedObjectNamesPrefix)) throw new ApplicationException("entityType.GetTableName().Contains(sanitizedObjectNamesPrefix)");
            if (entityType.IsTableExcludedFromMigrations()) throw new ApplicationException("entityType.IsTableExcludedFromMigrations()");

            foreach (var key in entityType.GetKeys())
            {
                string keyName = key.GetName() ?? key.GetDefaultName() ?? throw new ApplicationException("keyName == null");
                key.SetName(PrefixDbObjectName(sanitizedObjectNamesPrefix, keyName));
            }

            foreach (var checkConstraint in entityType.GetCheckConstraints())
            {
                string checkConstraintName = checkConstraint.Name
                    ?? checkConstraint.GetDefaultName()
                    ?? throw new ApplicationException("constraintName == null");
                checkConstraint.Name = PrefixDbObjectName(sanitizedObjectNamesPrefix, checkConstraintName);
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                string foreignKeyName = foreignKey.GetConstraintName()
                    ?? foreignKey.GetDefaultName()
                    ?? throw new ApplicationException("fkName == null");
                foreignKey.SetConstraintName(PrefixDbObjectName(sanitizedObjectNamesPrefix, foreignKeyName));
            }

            foreach (var index in entityType.GetIndexes())
            {
                string indexName = index.Name
                    ?? index.GetDatabaseName()
                    ?? index.GetDefaultDatabaseName()
                    ?? throw new ApplicationException("indexName == null");
                index.SetDatabaseName(PrefixDbObjectName(sanitizedObjectNamesPrefix, indexName));
            }
        }

        protected virtual void PrefixTableNamesAfterAllRelatedObjectNamesFixed(string sanitizedObjectNamesPrefix, ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
                if (entityType.GetTableName() != null && !entityType.IsTableExcludedFromMigrations())
                    PrefixTableNameAfterAllRelatedObjectNamesFixed(sanitizedObjectNamesPrefix, entityType);
        }

        protected virtual void PrefixTableNameAfterAllRelatedObjectNamesFixed(string sanitizedObjectNamesPrefix, IMutableEntityType entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            string tableName = entityType.GetTableName() ?? throw new ApplicationException("tableName == null");
            if (tableName.Contains(sanitizedObjectNamesPrefix)) throw new ApplicationException("tableName.Contains(sanitizedObjectNamesPrefix)");
            if (entityType.IsTableExcludedFromMigrations()) throw new ApplicationException("entityType.IsTableExcludedFromMigrations()");
            entityType.SetTableName(PrefixDbObjectName(sanitizedObjectNamesPrefix, tableName));
        }

        protected virtual string PrefixDbObjectName(string sanitizedObjectNamesPrefix, string objectName)
        {
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            if (objectName.Contains(sanitizedObjectNamesPrefix)) throw new ApplicationException("objectName.Contains(sanitizedObjectNamesPrefix)");

            string singlePrefixedObjectName = null;

            for (int i = 0; i < Configurators.Count; i++)
            {
                string prefixedObjectName = PrefixDbObjectName(sanitizedObjectNamesPrefix, objectName, Configurators[i]);
                if (prefixedObjectName != null)
                {
                    if (singlePrefixedObjectName != null)
                        throw new ApplicationException("singlePrefixedObjectName != null");
                    singlePrefixedObjectName = prefixedObjectName;
                }
            }

            return singlePrefixedObjectName ?? throw new ApplicationException("singlePrefixedObjectName");
        }

        protected virtual string PrefixDbObjectName(
            string sanitizedObjectNamesPrefix, string objectName, YourCompanyDbContextConfigurator configurator)
        {
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            if (objectName.Contains(sanitizedObjectNamesPrefix)) throw new ApplicationException("objectName.Contains(sanitizedObjectNamesPrefix)");
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            return configurator.PrefixDbObjectName(ConfiguratorsLoadingContext, sanitizedObjectNamesPrefix, objectName);
        }
    }
}