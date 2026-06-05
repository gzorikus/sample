namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public struct SortingKeyExtraValuePair<TExtraValue>
    {
        public SortingKey SortingKey { get; set; }
        public TExtraValue ExtraValue { get; set; }
    }
}