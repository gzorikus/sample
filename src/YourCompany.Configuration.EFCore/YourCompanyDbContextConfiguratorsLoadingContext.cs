using System;
using YourCompany.Configuration.EFCore.CollationAwareSorting;

namespace YourCompany.Configuration.EFCore
{
    public class YourCompanyDbContextConfiguratorsLoadingContext : YourCompanyPluginsLoadingContext
    {
        private Type _loadingConfigurationType;
        private Type _dbContextConfigurationType;
        private ICollationCompatibleComparersProvider _collationCompatibleComparersProvider;

        public bool IsDesignTime => RuntimeArgs != null;

        public int ModelCreatingAttempt { get; internal set; }

        public Type LoadingConfigurationType
        {
            get => _loadingConfigurationType ?? throw new ApplicationException("_loadingConfigurationType == null");
            internal set
            {
                if (_loadingConfigurationType != null) throw new ApplicationException("_loadingConfigurationType != null");
                _loadingConfigurationType = value ?? throw new ApplicationException("LoadingConfigurationType = null");
            }
        }

        public Type DbContextConfigurationType
        {
            get => _dbContextConfigurationType ?? throw new ApplicationException("_dbContextConfigurationType == null");
            internal set
            {
                if (_dbContextConfigurationType != null) throw new ApplicationException("_dbContextConfigurationType != null");
                _dbContextConfigurationType = value ?? throw new ApplicationException("DbContextConfigurationType = null");
            }
        }

        public ICollationCompatibleComparersProvider CollationCompatibleComparersProvider
        {
            get => _collationCompatibleComparersProvider ?? throw new ApplicationException("_collationCompatibleComparersProvider == null");
            internal set
            {
                if (_collationCompatibleComparersProvider != null) throw new ApplicationException("_collationCompatibleComparersProvider != null");
                _collationCompatibleComparersProvider = value ?? throw new ApplicationException("CollationCompatibleComparersProvider = null");
            }
        }
    }
}