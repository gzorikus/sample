using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey : SortingKeyTopology.IPropertiesPrefixFirstVisit
    {
        public abstract int VisitPrefixFirst(SortingKeyTopology.IPrefixFirstPropertiesVisitor visitor);
        public abstract int VisitPrefixFirst(IPrefixFirstPropertiesVisitor visitor);

        public interface IPrefixFirstPropertiesVisitor
        {
            ICollationAwareModelProvider ModelProvider { get; set; }

            void VisitProperty<TValue>(ValueHolding<TValue> sortingKey)
                where TValue : IEquatable<TValue>, IComparable<TValue>;

            void VisitProperty<TValue, TModelValue>(ValueHolding<TValue>.ConvertedFrom<TModelValue> sortingKey)
                where TValue : IEquatable<TValue>, IComparable<TValue>;
        }
    }
}