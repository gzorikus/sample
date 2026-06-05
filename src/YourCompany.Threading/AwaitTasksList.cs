using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourCompany.Threading
{
    public class AwaitTasksList : List<Task>
    {
        public AwaitTasksList() { }
        public AwaitTasksList(IEnumerable<Task> collection) : base(collection) { }
        public AwaitTasksList(int capacity) : base(capacity) { }

        public async Task WaitOnce()
        {
            List<Exception> inner = await WaitAndCollectExceptions();
            Clear();
            if (inner != null) throw new AggregateException(inner);
        }

        public async Task ThrowAndClear(Exception exception)
        {
            List<Exception> inner = await WaitAndCollectExceptions();
            inner?.Add(exception);
            Clear();
            if (inner != null) throw new AggregateException(inner);
            throw exception;
        }

        public async Task<List<Exception>> WaitAndCollectExceptions()
        {
            List<Exception> inner = null;

            for (int i = 0; i < Count; i++)
            {
                try
                {
                    await this[i];
                }
                catch (Exception ex)
                {
                    inner = inner ?? new List<Exception>(Count - i + 1);
                    inner.Add(ex);
                }
            }

            return inner;
        }
    }
}