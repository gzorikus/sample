using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class SpecifiedRun
        {
            public enum State : byte
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
        public static void EnsureIsConfiguration(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Configuration)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Configuration");
        }

        public static void EnsureIsAfterConfiguration(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value == RecordsBatchTransaction.SpecifiedRun.State.Configuration)
                throw new ApplicationException("value == RecordsBatchTransaction.SpecifiedRun.State.Configuration");
        }

        public static void EnsureIsStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Started)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Started");
        }

        public static void EnsureIsAuthorized(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Authorized)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Authorized");
        }

        public static void EnsureIsReadingStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ReadingStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ReadingStarted");
        }

        public static void EnsureIsRecordsBuildingStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.RecordsBuildingStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.RecordsBuildingStarted");
        }

        public static void EnsureIsChangesAssertionStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ChangesAssertionStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ChangesAssertionStarted");
        }

        public static void EnsureIsPersistChangesStarted(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesStarted)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.PersistStarted");
        }

        public static void EnsureIsPersistChangesFinished(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesFinished)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.PersistChangesFinished");
        }

        public static void EnsureIsDataAccessFinished(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.DataAccessFinished)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.DataAccessFinished");
        }

        public static void EnsureIsResultingRecordsBatchHandling(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.ResultingRecordsBatchHandling)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.ResultingRecordsBatchHandling");
        }

        public static void EnsureIsBeforeReturning(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value == RecordsBatchTransaction.SpecifiedRun.State.Returning)
                throw new ApplicationException("value == RecordsBatchTransaction.SpecifiedRun.State.Returning");
        }

        public static void EnsureIsReturning(this RecordsBatchTransaction.SpecifiedRun.State value)
        {
            if (value != RecordsBatchTransaction.SpecifiedRun.State.Returning)
                throw new ApplicationException("value != RecordsBatchTransaction.SpecifiedRun.State.Returning");
        }
    }
}