using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    public static partial class ComposableRecordsBatchTransaction
    {
        public abstract class IteratedInParallel<TRecord, TRecordData>
            : ConfiguredIdentically<TRecord, TRecordData>.NonRepositoryRecords,
            IIteratedInParallel
            where TRecord : class
            where TRecordData : class
        {
            private IEnumerator<RunOnceStep> _runOnceStepEnumeratorToDispose;

            IEnumerator<RunOnceStep> IIteratedInParallel.IterateRunOnceSteps(CancellationToken cancellationToken)
                => IterateRunOnceSteps(cancellationToken);

            protected new virtual IEnumerator<RunOnceStep> IterateRunOnceSteps(CancellationToken cancellationToken)
            {
                if (_runOnceStepEnumeratorToDispose != null) throw new ApplicationException("_runOnceStepEnumeratorToDispose != null");
                return _runOnceStepEnumeratorToDispose = base.IterateRunOnceSteps(cancellationToken).GetEnumerator();
            }

            protected override async Task Return(CancellationToken cancellationToken, Exception runException = null)
            {
                IDisposable disposeEnumerator = _runOnceStepEnumeratorToDispose;
                _runOnceStepEnumeratorToDispose = null;

                try
                {
                    await base.Return(cancellationToken, runException);
                }
                finally
                {
                    disposeEnumerator?.Dispose();
                }
            }
        }
    }
}