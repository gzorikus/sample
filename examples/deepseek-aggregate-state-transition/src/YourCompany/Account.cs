using System;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany
{
    public sealed class Account
    {
        internal const decimal BalanceDefault = 0m;

        private readonly IState<IAccountRecordData> _state;
        private decimal? _expectedOldBalance;

        internal Account(IStateAccess<IAccountRecordData> stateAccess)
            => _state = stateAccess?.Claim(OnTransactionCallback) ?? throw new ArgumentNullException(nameof(stateAccess));

        public decimal Balance => _state.GetFinishedAccessData().Balance ?? BalanceDefault;

        public void Withdraw(decimal amount, decimal expectedOldBalance)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, message: null);
            if (expectedOldBalance < 0) throw new ArgumentOutOfRangeException(nameof(expectedOldBalance), expectedOldBalance, message: null);
            if (_expectedOldBalance.HasValue) throw new ApplicationException("_expectedOldBalance.HasValue");

            decimal newBalance;
            checked { newBalance = expectedOldBalance - amount; }
            if (newBalance < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, null);
            _state.GetSettingPropertiesBeforeDataChanging().Balance = newBalance;
            _expectedOldBalance = expectedOldBalance;
        }

        public void Deposit(decimal amount, decimal expectedOldBalance)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, message: null);
            if (expectedOldBalance < 0) throw new ArgumentOutOfRangeException(nameof(expectedOldBalance), expectedOldBalance, message: null);
            if (_expectedOldBalance.HasValue) throw new ApplicationException("_expectedOldBalance.HasValue");

            decimal newBalance;
            checked { newBalance = expectedOldBalance + amount; }
            _state.GetSettingPropertiesBeforeDataChanging().Balance = newBalance;
            _expectedOldBalance = expectedOldBalance;
        }

        private void OnTransactionCallback(object sender, EventArgs e)
        {
            if (e is TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging)
            {
                // nothing
            }
            else if (e is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual)
            {
                int beforeChangesCount = _state.CheckHasSingleSpecificationToMatchWithoutDataChanges(greaterOrEqual) ? 1 : 0;
                int afterChangesCount = _state.CheckHasSingleSpecificationForDataChanging(greaterOrEqual) ? 1 : 0;

                if (beforeChangesCount + afterChangesCount > 1)
                    throw new ApplicationException("e is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual && beforeChangesCount + afterChangesCount > 1");

                greaterOrEqual.AssertChangeMode(beforeChangesCount, afterChangesCount);
            }
            else if (e is AccountSpecifications.Triggering.BalanceThreshold.LessThan less)
            {
                int beforeChangesCount = _state.CheckHasSingleSpecificationToMatchWithoutDataChanges(less) ? 1 : 0;
                int afterChangesCount = _state.CheckHasSingleSpecificationForDataChanging(less) ? 1 : 0;

                if (beforeChangesCount + afterChangesCount > 1)
                    throw new ApplicationException("e is AccountSpecifications.Triggering.BalanceThreshold.LessThan && beforeChangesCount + afterChangesCount > 1");

                if (afterChangesCount > 0)
                    throw new ApplicationException("e is AccountSpecifications.Triggering.BalanceThreshold.LessThan && afterChangesCount > 0");
            }
            else if (e is TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking)
            {
                decimal balance = _state.GetLockedRecordData().Balance ?? throw new ApplicationException("!_state.GetLockedRecordData().Balance.HasValue");
                if (_expectedOldBalance.HasValue && balance != _expectedOldBalance.Value)
                    throw new AccountException.ConcurrentBalanceModification(_expectedOldBalance.Value, balance);
            }
            else if (e is TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess)
            {
                decimal balance = _state.GetFinishedAccessData().Balance ?? throw new ApplicationException("!_state.GetFinishedAccessData().Balance.HasValue");
                if (balance < 0) throw new ApplicationException("e is TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess && balance < 0");
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(e), e, message: null);
            }
        }
    }
}