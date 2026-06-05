using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey : SortingKeyTopology.IPropertiesPrefixFirstVisit
    {
        public abstract int VisitPrefixFirst(SortingKeyTopology.IPrefixFirstPropertiesVisitor visitor);
        internal abstract int VisitPrefixFirst(IPrefixFirstPropertiesVisitor visitor);

        internal interface IPrefixFirstPropertiesVisitor
        {
            ICollationAwareModelProvider ModelProvider { get; set; }

            void VisitProperty<TValue>(ValueHolding<TValue> sortingKey)
                where TValue : IEquatable<TValue>, IComparable<TValue>;

            void VisitProperty<TValue, TModelValue>(ValueHolding<TValue>.ConvertedFrom<TModelValue> sortingKey)
                where TValue : IEquatable<TValue>, IComparable<TValue>;
        }
    }
}