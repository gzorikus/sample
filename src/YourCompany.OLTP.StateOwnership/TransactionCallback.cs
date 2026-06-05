using System;

namespace YourCompany.OLTP.StateOwnership
{
    public static class TransactionCallback
    {
        public static T GetExtraInterface<T>(this object sender) where T : class
        {
            var senderWithExtraInterfaces = sender as Sender.IWithExtraInterfaces
                ?? throw new ApplicationException("senderWithExtraInterfaces == null");
            if (!senderWithExtraInterfaces.TryGetExtraInterface<T>(out var extraInterface))
                throw new ApplicationException("!senderWithExtraInterfaces.TryGetExtraInterface<T>(out var extraInterface)");
            return extraInterface;
        }

        public static bool TryGetExtraInterface<T>(this Sender.IWithExtraInterfaces sender, out T extraInterface)
            where T : class
        {
            sender.TryGetExtraInterfaceImplementor(typeof(T), out object extraInterfaceObj);
            extraInterface = (T)extraInterfaceObj;
            return extraInterface != null;
        }

        public static class Sender
        {
            public static object Default { get; } = new object();

            public interface IWithExtraInterfaces
            {
                bool TryGetExtraInterfaceImplementor(Type type, out object implementor);
            }
        }

        public abstract class ForStateAssertion : EventArgs
        {
            private ForStateAssertion() { }

            public class TriggeredBeforeDataChanging : ForStateAssertion
            {
                public static TriggeredBeforeDataChanging Default { get; } = new TriggeredBeforeDataChanging();
            }

            public class TriggeredAfterRecordDataLocking : ForStateAssertion
            {
                public static TriggeredAfterRecordDataLocking Default { get; } = new TriggeredAfterRecordDataLocking();
            }

            public class TriggeredAfterDataAccess : ForStateAssertion
            {
                public static TriggeredAfterDataAccess Default { get; } = new TriggeredAfterDataAccess();
            }
        }
    }
}