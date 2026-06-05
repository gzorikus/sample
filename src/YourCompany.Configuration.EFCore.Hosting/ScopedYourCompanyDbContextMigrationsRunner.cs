using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YourCompany.Configuration.EFCore.Hosting
{
    public class ScopedYourCompanyDbContextMigrationsRunner<TConfiguration> : IHostedService
        where TConfiguration : YourCompanyDbContextConfiguration,
            YourCompanyDbContextConfigurationInterfaces.IMigrationRunner,
            new()
    {
        private readonly IServiceProvider _provider;

        public ScopedYourCompanyDbContextMigrationsRunner(IServiceProvider provider)
            => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _provider.CreateAsyncScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<YourCompanyDbContext<TConfiguration>>>();
            await using var context = await factory.CreateDbContextAsync(cancellationToken);
            if (context.Configuration.MigrateDatabaseOnStart) await context.Database.MigrateAsync(cancellationToken);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}