using System;
using System.Threading.Tasks;
using Npgsql;
using YourCompany.ConsoleProgram.UseCases;
using YourCompany.Dapper;
using YourCompany.Dapper.PostgreSQL;

namespace YourCompany.ConsoleProgram
{
    internal static class Program
    {
        static async Task Main()
        {
            const string connectionString = "Host=localhost;Port=5432;" +
                "Database=YourCompanyDemoDatabase;Username=YourCompanyDemoUser;" +
                "SearchPath=deepseek-aggregate-state-transition;" +
                "Options=-c lock_timeout=100;" +
                "Include Error Detail=true";

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            await using var dataSource = dataSourceBuilder.Build();

            var transactionFactory = () => new NpgsqlAccountsBatchTransaction(dataSource);

            var sender = Guid.NewGuid();
            var beneficiary = Guid.NewGuid();
            var poorman = Guid.NewGuid();

            await using (var topupTransaction = transactionFactory())
            {
                topupTransaction.ById(new DapperAccountId(sender)).Deposit(amount: 500m, expectedOldBalance: 0);
                topupTransaction.ById(new DapperAccountId(beneficiary)).Deposit(amount: 300m, expectedOldBalance: 0);
                topupTransaction.ById(new DapperAccountId(poorman)).Deposit(amount: 1m, expectedOldBalance: 0);
                await topupTransaction.Run();
            }

            var transfer = new TransferFunds(
                fromPublicId: sender,
                toPublicId: beneficiary,
                amount: 100m,
                fromExpectedBalance: 500m,
                toExpectedBalance: 300m,
                transactionFactory);
            await transfer.ExecuteAsync();
            Console.WriteLine("Transfer succeeded");

            var giveBagels = new GiveBagelsToPoorAccounts(transactionFactory);
            int count = await giveBagels.ExecuteAsync();
            Console.WriteLine($"Bagels given to {count} poor accounts.");
        }
    }
}