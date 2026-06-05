using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        internal abstract partial class SpecifiedRun
        {
            internal enum State : byte
            {
                Configuration = 0,
                Started,
                Authorized,
                ReadingStarted,
                RecordsBuildingStarted,
                ChangesAssertionStarted,
                PersistChangesStarted,
                PersistChangesFinished,
                DataAccessFinished,
                ResultingRecordsBatchHandling,
                Returning
            }
        }
    }

    public static class RecordsBatchTransactionSpecifiedRunStateExtensions
    {
        internal static void EnsureIsConfiguration(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Configuration)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Configuration");
        }

        internal static void EnsureIsAfterConfiguration(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value == RecordsBatchTransaction.SpecifiedRun.State.Configuration)
                throw new ApplicationException("value == RecordsBatchTransaction.SpecifiedRun.State.Configuration");
        }

        internal static void EnsureIsStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Started)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Started");
        }

        internal static void EnsureIsAuthorized(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Authorized)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Authorized");
        }

        internal static void EnsureIsReadingStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ReadingStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ReadingStarted");
        }

        internal static void EnsureIsRecordsBuildingStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.RecordsBuildingStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.RecordsBuildingStarted");
        }

        internal static void EnsureIsChangesAssertionStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ChangesAssertionStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ChangesAssertionStarted");
        }

        internal static void EnsureIsPersistChangesStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.PersistStarted");
        }

        internal static void EnsureIsPersistChangesFinished(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesFinished)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesFinished");
        }

        internal static void EnsureIsDataAccessFinished(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.DataAccessFinished)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.DataAccessFinished");
        }

        internal static void EnsureIsResultingRecordsBatchHandling(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ResultingRecordsBatchHandling)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ResultingRecordsBatchHandling");
        }

        internal static void EnsureIsBeforeReturning(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value == RecordsBatchTransaction.SpecifiedRun.State.Returning)
                throw new ApplicationException("value == RecordsBatchTransaction.SpecifiedRun.State.Returning");
        }

        internal static void EnsureIsReturning(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Returning)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Returning");
        }
    }
}