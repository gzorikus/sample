using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.Persistence
{
    internal static class AccountSpecifications
    {
        internal static Triggering.BalanceThreshold.LessThan LessThan(decimal threshold) =>
            new Triggering.BalanceThreshold.LessThan(threshold);

        internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualNoChange(decimal threshold) =>
            new Triggering.BalanceThreshold.GreaterOrEqual(threshold, Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.NoChange);

        internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualAfterIncrement(decimal threshold) =>
            new Triggering.BalanceThreshold.GreaterOrEqual(threshold, Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Increment);

        internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualAfterSet(decimal threshold) =>
            new Triggering.BalanceThreshold.GreaterOrEqual(threshold, Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Set);

        internal abstract class Triggering : EventArgs, ISpecification<IAccountRecordData>
        {
            private Triggering() { }

            bool ISpecification<IAccountRecordData>.Match(IAccountRecordData recordData)
            {
                if (recordData == null) throw new ArgumentNullException(nameof(recordData));
                return MatchPresent(recordData);
            }

            protected abstract bool MatchPresent(IAccountRecordData recordData);

            internal abstract class BalanceThreshold : Triggering
            {
                public decimal Threshold { get; }

                private BalanceThreshold(decimal threshold)
                {
                    if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold), threshold, message: null);
                    Threshold = threshold;
                }

                protected sealed override bool MatchPresent(IAccountRecordData recordData)
                {
                    decimal balance = recordData.Balance ?? throw new ApplicationException("!recordData.Balance.HasValue");
                    return MatchBalance(balance);
                }

                protected abstract bool MatchBalance(decimal balance);

                internal sealed class LessThan : BalanceThreshold
                {
                    internal LessThan(decimal threshold) : base(threshold) { }
                    protected override bool MatchBalance(decimal balance) => balance < Threshold;
                }

                internal sealed class GreaterOrEqual : BalanceThreshold
                {
                    internal ChangeDataToMatchMode ChangeMode { get; }

                    internal GreaterOrEqual(decimal threshold, ChangeDataToMatchMode mode) : base(threshold)
                    {
                        ChangeMode = mode;
                    }

                    protected override bool MatchBalance(decimal balance) => balance >= Threshold;

                    internal void AssertChangeMode(int usedBeforeChanges, int usedAfterChanges)
                    {
                        switch (ChangeMode)
                        {
                            case ChangeDataToMatchMode.NoChange:
                                bool isValidNoChange = usedBeforeChanges == 1 && usedAfterChanges == 0;
                                if (!isValidNoChange) throw new ApplicationException("case ChangeDataToMatchMode.NoChange && !isValidNoChange");
                                break;
                            case ChangeDataToMatchMode.Increment:
                                bool isValidIncrement = usedBeforeChanges == 0 && usedAfterChanges == 1;
                                if (!isValidIncrement) throw new ApplicationException("case ChangeDataToMatchMode.Increment && !isValidIncrement");
                                break;
                            case ChangeDataToMatchMode.Set:
                                bool isValidSet = usedBeforeChanges == 0 && usedAfterChanges == 1;
                                if (!isValidSet) throw new ApplicationException("case ChangeDataToMatchMode.Set && !isValidSet");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(ChangeMode), ChangeMode, message: null);
                        }
                    }

                    internal enum ChangeDataToMatchMode
                    {
                        NoChange,
                        Increment,
                        Set
                    }
                }
            }
        }
    }
}