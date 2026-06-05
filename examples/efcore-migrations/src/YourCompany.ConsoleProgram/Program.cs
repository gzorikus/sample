using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using Microsoft.Extensions.DependencyInjection;
=======
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YourCompany.Configuration.EFCore;
using YourCompany.Configuration.EFCore.Hosting;
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
using YourCompany.EFCore;
using YourCompany.EFCore.NeverPretendingToBeYourDomainModel;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.ConsoleProgram;

internal static class Program
{
    public static async Task Main(string[] args)
    {
<<<<<<< HEAD
        var services = new ServiceCollection();
        services.AddYourCompanyEFCoreScopedDbContextFactory<CustomConfiguration>();
        services.AddYourCompanyEFCoreScopedDbContextFactoryLoadedDomainTypes<CustomConfiguration>();
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<YourCompanyDbContext<CustomConfiguration>>>();
=======
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddYourCompanyConfiguration();
        builder.Services.AddSingleton<
            IDbContextFactory<YourCompanyDbContext<CustomConfiguration>>,
            YourCompanyDbContextFactory<YourCompanyDbContextFactoryLoadingConfiguration, CustomConfiguration>.RunTime>();
        builder.Services.AddHostedService<ScopedYourCompanyDbContextMigrationsRunner<CustomConfiguration>>();
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run

        using var host = builder.Build();
        var factory = host.Services.GetRequiredService<IDbContextFactory<YourCompanyDbContext<CustomConfiguration>>>();

        await host.StartAsync();

        try
        {
            await using var context = await factory.CreateDbContextAsync();
            context.Add(new EFCoreConventionalEntity { Column1 = 1, Column2 = "Hello, World!" });
            await context.SaveChangesAsync();
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
