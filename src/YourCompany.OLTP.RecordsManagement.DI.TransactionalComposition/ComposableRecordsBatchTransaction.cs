using System;
using System.Collections.Generic;
using System.Threading;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal static partial class ComposableRecordsBatchTransaction
    {
        internal interface IComposingRecords
        {
            ComposableRecordTypeInfo RecordTypeInfo { get; set; }
            bool HandleTriggering(EventArgs parametersToJoinTransactionWith, object atComposedRecord);
        }

        internal interface IConfiguredIdentically : IComposingRecords
        {
            bool Add(RecordsBatchTransactionSpecification specification);
            bool AddSupportedRecordDataSpecification(RecordsBatchTransactionSpecification specification);
            void FinishConfiguration(bool includeForChanges);
        }

        internal interface IIteratedInParallel : IComposingRecords
        {
            IEnumerator<RecordsBatchTransaction.SpecifiedRun.RunOnceStep> IterateRunOnceSteps(
                CancellationToken cancellationToken);
        }
    }
}