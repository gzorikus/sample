using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        internal abstract partial class SpecifiedRun
        {
            internal readonly struct RunOnceStep
            {
                internal State State { get; }
                internal Task AsyncStepTask { get; }

                internal RunOnceStep(State state, Task asyncStepTask = null)
                {
                    State = state;
                    AsyncStepTask = asyncStepTask;
                }
            }
        }
    }
}