using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        private Task _lock;

        public async Task Lock(Task exclusive)
        {
            if (exclusive == null) throw new ArgumentNullException(nameof(exclusive));
            while (true)
            {
                Task alreadyLocked = Interlocked.CompareExchange(ref _lock, exclusive, null);
                if (exclusive == alreadyLocked) throw new ApplicationException("exclusive == alreadyLocked");
                if (alreadyLocked == null) break;
                await alreadyLocked;
                var alreadyLockedRemained = Interlocked.CompareExchange(ref _lock, exclusive, alreadyLocked);
                if (alreadyLocked == alreadyLockedRemained) break;
            }
        }

        protected void EnsureLocked()
        {
            var currentLock = Interlocked.CompareExchange(ref _lock, null, null) ?? throw new ApplicationException("currentLock == null");
            if (currentLock.Status != TaskStatus.Running) throw new ApplicationException("currentLock.Status != TaskStatus.Running");
        }
    }
}