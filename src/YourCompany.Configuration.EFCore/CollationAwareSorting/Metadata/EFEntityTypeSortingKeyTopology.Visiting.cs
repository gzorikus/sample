using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata
{
    public sealed partial class EFEntityTypeSortingKeyTopology : SortingKeyTopology.IPropertiesPrefixFirstVisit
    {
        private Action<SortingKeyTopology.IPrefixFirstPropertiesVisitor, SortingKeyTopology.ILastProperty>
            _publicCompiledVisitPropertySameType;

        private Action<SortingKeyTopology.IPrefixFirstPropertiesVisitor, SortingKeyTopology.ILastProperty>
            _publicCompiledVisitPropertyConverted;

        private Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
            _internalCompiledVisitPropertySameType;

        private Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
            _internalCompiledVisitPropertyConverted;

        public int VisitPrefixFirst(SortingKeyTopology.IPrefixFirstPropertiesVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            int count = Prefix?.VisitPrefixFirst(visitor) ?? 0;
            visitor.ModelProvider ??= ModelProvider;
            if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");

            var valueConverter = Property.GetValueConverter();
            if (valueConverter != null && valueConverter.ModelClrType != PropertyValueType)
                throw new ApplicationException("valueConverter != null && valueConverter.ModelClrType != PropertyValueType");

            if (valueConverter == null || valueConverter.ModelClrType == valueConverter.ProviderClrType)
            {
                _publicCompiledVisitPropertySameType ??= SortingKeyTopology
                    .PrefixFirstPropertiesVisitCache
                    .CompileVisitPropertySameType(PropertyValueType);
                _publicCompiledVisitPropertySameType(visitor, this);
            }
            else
            {
                _publicCompiledVisitPropertyConverted ??= SortingKeyTopology
                    .PrefixFirstPropertiesVisitCache
                    .CompileVisitPropertyConverted(PropertyValueType, valueConverter.ProviderClrType);
                _publicCompiledVisitPropertyConverted(visitor, this);
            }

            return count + 1;
        }

        public int VisitPrefixFirst(IPrefixFirstPropertiesVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            int count = Prefix?.VisitPrefixFirst(visitor) ?? 0;
            visitor.ModelProvider ??= ModelProvider;
            visitor.EntityType ??= EntityType;
            if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
            if (visitor.EntityType != EntityType) throw new ApplicationException("visitor.EntityType != EntityType");

            var valueConverter = Property.GetValueConverter();
            if (valueConverter != null && valueConverter.ModelClrType != PropertyValueType)
                throw new ApplicationException("valueConverter != null && valueConverter.ModelClrType != PropertyValueType");

            if (valueConverter == null || valueConverter.ModelClrType == valueConverter.ProviderClrType)
            {
                _internalCompiledVisitPropertySameType ??= PrefixFirstPropertiesVisitCache
                    .CompileVisitPropertySameType(PropertyValueType);
                _internalCompiledVisitPropertySameType(visitor, this);
            }
            else
            {
                _internalCompiledVisitPropertyConverted ??= PrefixFirstPropertiesVisitCache
                    .CompileVisitPropertyConverted(PropertyValueType, valueConverter.ProviderClrType);
                _internalCompiledVisitPropertyConverted(visitor, this);
            }

            return count + 1;
        }

        public interface IPrefixFirstPropertiesVisitor
        {
            ICollationAwareModelProvider ModelProvider { get; set; }
            IEntityType EntityType { get; set; }

            void VisitProperty<TValue>(ILastProperty topology)
                where TValue : IEquatable<TValue>, IComparable<TValue>;

            void VisitProperty<TValue, TModelValue>(ILastProperty topology)
                where TValue : IEquatable<TValue>, IComparable<TValue>;
        }
    }
}