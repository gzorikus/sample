using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public interface ICollationCompatibleComparersProvider
    {
        IEqualityComparer<string> GetCollationCompatibleStringEqualityComparer(string collationNameOrProviderDefault = null);
        IComparer<string> GetCollationCompatibleStringComparer(string collationNameOrProviderDefault = null);
    }

    internal static class CollationCompatibleComparersProviderExtensions
    {
        internal static ICollationCompatibleComparersProvider ToCollationCompatibleComparersProvider(
            this IReadOnlyList<YourCompanyDbContextConfigurator> configurators,
            YourCompanyDbContextConfiguratorsLoadingContext loadingContext)
            => new CollationCompatibleComparersProvider(configurators, loadingContext);

        private sealed class CollationCompatibleComparersProvider : ICollationCompatibleComparersProvider
        {
            private readonly IReadOnlyList<YourCompanyDbContextConfigurator> _configurators;
            private readonly YourCompanyDbContextConfiguratorsLoadingContext _loadingContext;

            internal CollationCompatibleComparersProvider(
                IReadOnlyList<YourCompanyDbContextConfigurator> configurators,
                YourCompanyDbContextConfiguratorsLoadingContext loadingContext)
            {
                _configurators = configurators ?? throw new ArgumentNullException(nameof(configurators));
                _loadingContext = loadingContext ?? throw new ArgumentNullException(nameof(loadingContext));
            }

            IEqualityComparer<string> ICollationCompatibleComparersProvider.GetCollationCompatibleStringEqualityComparer(
                string collationNameOrProviderDefault)
            {
                IEqualityComparer<string> singleComparer = null;

                for (int i = 0; i < _configurators.Count; i++)
                {
                    var comparer = _configurators[i].GetCollationCompatibleStringEqualityComparer(
                        _loadingContext, collationNameOrProviderDefault);

                    if (comparer != null)
                    {
                        if (singleComparer != null) throw new ApplicationException("singleComparer != null");
                        singleComparer = comparer;
                    }
                }

                return singleComparer ?? throw new ApplicationException("singleComparer == null");
            }

            IComparer<string> ICollationCompatibleComparersProvider.GetCollationCompatibleStringComparer(
                string collationNameOrProviderDefault)
            {
                IComparer<string> singleComparer = null;

                for (int i = 0; i < _configurators.Count; i++)
                {
                    var comparer = _configurators[i].GetCollationCompatibleStringComparer(
                        _loadingContext, collationNameOrProviderDefault);

                    if (comparer != null)
                    {
                        if (singleComparer != null) throw new ApplicationException("singleComparer != null");
                        singleComparer = comparer;
                    }
                }

                return singleComparer ?? throw new ApplicationException("singleComparer == null");
            }
        }
    }
}