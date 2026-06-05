using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace YourCompany.Configuration
{
    public class YourCompanyPluginsLoadingContext
    {
        public IReadOnlyList<string> RuntimeArgs { get; init; }
        public IConfiguration Configuration { get; init; }
    }
}