using System;

namespace YourCompany.OLTP.StateOwnership.EventsProducing
{
    public static class EventProducingTransactionCallback
    {
        public static void ProduceEvent<TEvent>(this object sender, TEvent @event)
        {
            var extraInterface = sender.GetExtraInterface<IProducingSenderExtraInterface>();
            var delayFor = (@event as EventsProducingStrategies.IDelaying)?.DelayFor;
            var scheduleFor = (@event as EventsProducingStrategies.IScheduling)?.ScheduleFor;
            if (delayFor.HasValue)
            {
                if (scheduleFor.HasValue) throw new ApplicationException("delayFor.HasValue && scheduleFor.HasValue");
                extraInterface.Produce(@event, delayFor);
            }
            else if (scheduleFor.HasValue)
            {
                extraInterface.Produce(@event, scheduleFor);
            }
            else
            {
                extraInterface.Produce(@event);
            }
        }

        public interface IProducingSenderExtraInterface
        {
            void Produce<TEvent>(TEvent @event);
            void Produce<TEvent>(TEvent @event, TimeSpan? delayFor);
            void Produce<TEvent>(TEvent @event, DateTime? scheduleFor);
        }
    }
}