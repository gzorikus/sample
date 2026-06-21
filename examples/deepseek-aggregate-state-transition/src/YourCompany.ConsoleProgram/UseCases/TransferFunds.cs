using System;
using System.Threading.Tasks;
using YourCompany.Dapper;

namespace YourCompany.ConsoleProgram.UseCases
{
    internal sealed class TransferFunds
    {
        private readonly Guid _fromPublicId;
        private readonly Guid _toPublicId;
        private readonly decimal _amount;
        private readonly decimal _fromExpectedBalance;
        private readonly decimal _toExpectedBalance;
        private readonly Func<DapperAccountsBatchTransaction> _transactionFactory;

        internal TransferFunds(
            Guid fromPublicId,
            Guid toPublicId,
            decimal amount,
            decimal fromExpectedBalance,
            decimal toExpectedBalance,
            Func<DapperAccountsBatchTransaction> transactionFactory)
        {
            _fromPublicId = fromPublicId;
            _toPublicId = toPublicId;
            _amount = amount;
            _fromExpectedBalance = fromExpectedBalance;
            _toExpectedBalance = toExpectedBalance;
            _transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
        }

        internal async Task ExecuteAsync()
        {
            await using var transaction = _transactionFactory();
            var fromAccount = transaction.ById(new DapperAccountId(_fromPublicId));
            var toAccount = transaction.ById(new DapperAccountId(_toPublicId));

            fromAccount.Withdraw(_amount, _fromExpectedBalance);
            toAccount.Deposit(_amount, _toExpectedBalance);

            await transaction.Run();
            if (transaction.Accounts.Count != 2) throw new ApplicationException("transaction.Accounts.Count == 2");
        }
    }
}