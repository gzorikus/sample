using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopology
    {
        public interface IKeyModel
        {
            ICollationAwareModelProvider ModelProvider { get; }
        }

        public interface ILastProperty : IKeyModel, IPropertiesPrefixFirstVisit
        {
            ILastProperty Prefix { get; }
            int PrefixKeysCount { get; }
            bool MultiplePropertyOwners { get; }
            bool UniquePropertyNames { get; }
            PropertiesOwner PropertyOwner { get; }
            string PropertyName { get; }
            Type PropertyValueType { get; }
            bool Descending { get; }
        }

        public interface IPropertiesPrefixFirstVisit
        {
            int VisitPrefixFirst(IPrefixFirstPropertiesVisitor visitor);
        }

        public interface IPrefixFirstPropertiesVisitor
        {
            ICollationAwareModelProvider ModelProvider { get; set; }

            void VisitProperty<TValue>(ILastProperty topology)
                where TValue : IEquatable<TValue>, IComparable<TValue>;

            void VisitProperty<TValue, TModelValue>(ILastProperty topology)
                where TValue : IEquatable<TValue>, IComparable<TValue>;
        }
    }
}