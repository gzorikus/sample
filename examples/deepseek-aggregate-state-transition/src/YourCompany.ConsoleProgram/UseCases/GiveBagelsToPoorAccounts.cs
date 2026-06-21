using System;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Dapper;
using YourCompany.Persistence;

namespace YourCompany.ConsoleProgram.UseCases
{
    internal sealed class GiveBagelsToPoorAccounts
    {
        private const decimal PowertyThreshold = 10m;
        private const decimal BagelsToGive = 100m;

        private readonly Func<DapperAccountsBatchTransaction> _transactionFactory;

        internal GiveBagelsToPoorAccounts(Func<DapperAccountsBatchTransaction> transactionFactory)
        {
            _transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
        }

        internal async Task<int> ExecuteAsync(CancellationToken ct = default)
        {
            int totalUpdated = 0;
            while (!ct.IsCancellationRequested)
            {
                await using var transaction = _transactionFactory();
                transaction.ConfigureBatch(batchSize: 50, useForUpdateSkipLocked: true);
                transaction.MatchBeforeDataChanging(AccountSpecifications.LessThan(PowertyThreshold));
                transaction.ChangeDataToMatch(AccountSpecifications.GreaterOrEqualAfterIncrement(BagelsToGive));
                await transaction.Run();
                totalUpdated += transaction.Accounts.Count;
                if (transaction.Accounts.Count < transaction.BatchSize) break;
            }
            return totalUpdated;
        }
    }
}