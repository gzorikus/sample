using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YourCompany.Configuration.EFCore.Hosting;
using YourCompany.EFCore;
using YourCompany.EFCore.NeverPretendingToBeYourDomainModel;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.ConsoleProgram;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddYourCompanyConfiguration();
        builder.Services.AddYourCompanyEFCoreScopedDbContextFactory<CustomConfiguration>();
        builder.Services.AddYourCompanyEFCoreScopedDbContextFactoryLoadedDomainTypes<CustomConfiguration>();
        builder.Services.AddHostedService<ScopedYourCompanyDbContextMigrationsRunner<CustomConfiguration>>();

        using var host = builder.Build();
        await using var scope = host.Services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<YourCompanyDbContext<CustomConfiguration>>>();

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
