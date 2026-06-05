using System;

namespace YourCompany.OLTP.StateOwnership.EventsProducing
{
    public static class EventsProducingStrategies
    {
        public interface IDelaying
        {
            TimeSpan? DelayFor { get; }
        }

        public interface IScheduling
        {
            DateTime? ScheduleFor { get; }
        }
    }
}