using System;

namespace YourCompany.OLTP.StateOwnership.TransactionalComposition
{
    public static class TransactionalCompositionTransactionCallback
    {
        public static void Trigger(this object sender, EventArgs parametersToJoinTransactionWith, object atComposedRecord)
        {
            if (parametersToJoinTransactionWith == null) throw new ArgumentNullException(nameof(parametersToJoinTransactionWith));
            if (atComposedRecord == null) throw new ArgumentNullException(nameof(atComposedRecord));
            var extraInterface = sender.GetExtraInterface<ITriggeringSenderExtraInterface>();
            extraInterface.Trigger(parametersToJoinTransactionWith, atComposedRecord);
        }

        internal interface ITriggeringSenderExtraInterface
        {
            void Trigger(EventArgs parametersToJoinTransactionWith, object atComposedRecord);
        }
    }
}