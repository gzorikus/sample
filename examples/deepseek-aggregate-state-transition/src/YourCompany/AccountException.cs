using System;

namespace YourCompany
{
    public abstract class AccountException : Exception
    {
        private AccountException() { }
        // private AccountException(Exception inner) : base(null, inner) { }

        public sealed class ConcurrentBalanceModification : AccountException
        {
            public decimal ExpectedOldBalance { get; }
            public decimal ActualOldBalance { get; }

            internal ConcurrentBalanceModification(decimal expected, decimal actual)
            {
                ExpectedOldBalance = expected;
                ActualOldBalance = actual;
            }
        }

        // Future extension point (if needed, public constructor for cross‑aggregate)
        // public abstract class Extension : AccountException
        // {
        //     protected Extension() { }
        // }
    }
}