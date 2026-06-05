using System;
using System.Collections.Generic;
using System.Threading;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    public static partial class ComposableRecordsBatchTransaction
    {
        public interface IComposingRecords
        {
            ComposableRecordTypeInfo RecordTypeInfo { get; }
            bool HandleTriggering(EventArgs parametersToJoinTransactionWith, object atComposedRecord);
        }

        public interface IConfiguredIdentically : IComposingRecords
        {
            bool Add(RecordsBatchTransactionSpecification specification);
            bool AddSupportedRecordDataSpecification(RecordsBatchTransactionSpecification specification);
            void FinishConfiguration(bool includeForChanges);
        }

        public interface IIteratedInParallel : IComposingRecords
        {
            IEnumerator<RecordsBatchTransaction.SpecifiedRun.RunOnceStep> IterateRunOnceSteps(
                CancellationToken cancellationToken);
        }
    }
}