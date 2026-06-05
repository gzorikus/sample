using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YourCompany.EFCore;
using YourCompany.EFCore.NeverPretendingToBeYourDomainModel;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.ConsoleProgram;

internal static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddYourCompanyEFCoreScopedDbContextFactory<CustomConfiguration>();
        services.AddYourCompanyEFCoreScopedDbContextFactoryLoadedDomainTypes<CustomConfiguration>();
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<YourCompanyDbContext<CustomConfiguration>>>();

        await using var context = await factory.CreateDbContextAsync();
        context.Add(new EFCoreConventionalEntity { Column1 = 1, Column2 = "Hello, World!" });
        await context.SaveChangesAsync();
    }
}
