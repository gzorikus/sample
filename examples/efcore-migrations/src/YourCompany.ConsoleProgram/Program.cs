using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourCompany.Configuration.EFCore;
using YourCompany.EFCore;
using YourCompany.EFCore.NeverPretendingToBeYourDomainModel;

namespace YourCompany.ConsoleProgram;

internal static class Program
{
    public static async Task Main()
    {
        IDbContextFactory<YourCompanyDbContext<CustomConfiguration>> factory
            = new YourCompanyDbContextFactory<YourCompanyDbContextFactoryLoadingConfiguration, CustomConfiguration>.RunTime();

        await using var context = await factory.CreateDbContextAsync();
        context.Add(new EFCoreConventionalEntity { Column1 = 1, Column2 = "Hello, World!" });
        await context.SaveChangesAsync();
    }
}
