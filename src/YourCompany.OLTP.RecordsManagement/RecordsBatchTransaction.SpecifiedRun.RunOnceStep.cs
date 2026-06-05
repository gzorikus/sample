using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class SpecifiedRun
        {
            public readonly struct RunOnceStep
            {
                public State State { get; }
                public Task AsyncStepTask { get; }

                public RunOnceStep(State state, Task asyncStepTask = null)
                {
                    State = state;
                    AsyncStepTask = asyncStepTask;
                }
            }
        }
    }
}