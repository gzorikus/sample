using System;

namespace YourCompany.OLTP.StateOwnership
{
    public static class TransactionCallback
    {
        internal static T GetExtraInterface<T>(this object sender) where T : class
        {
            var senderWithExtraInterfaces = sender as Sender.IWithExtraInterfaces
                ?? throw new ApplicationException("senderWithExtraInterfaces == null");
            if (!senderWithExtraInterfaces.TryGetExtraInterface<T>(out var extraInterface))
                throw new ApplicationException("!senderWithExtraInterfaces.TryGetExtraInterface<T>(out var extraInterface)");
            return extraInterface;
        }

        internal static bool TryGetExtraInterface<T>(this Sender.IWithExtraInterfaces sender, out T extraInterface)
            where T : class
        {
            sender.TryGetExtraInterfaceImplementor(typeof(T), out object extraInterfaceObj);
            extraInterface = (T)extraInterfaceObj;
            return extraInterface != null;
        }

        internal static class Sender
        {
            internal static object Default { get; } = new object();

            internal interface IWithExtraInterfaces
            {
                bool TryGetExtraInterfaceImplementor(Type type, out object implementor);
            }
        }

        public abstract class ForStateAssertion : EventArgs
        {
            private ForStateAssertion() { }

            public class TriggeredBeforeDataChanging : ForStateAssertion
            {
                internal static TriggeredBeforeDataChanging Default { get; } = new TriggeredBeforeDataChanging();

                internal TriggeredBeforeDataChanging() { }
            }

            public class TriggeredAfterRecordDataLocking : ForStateAssertion
            {
                internal static TriggeredAfterRecordDataLocking Default { get; } = new TriggeredAfterRecordDataLocking();

                internal TriggeredAfterRecordDataLocking() { }
            }

            public class TriggeredAfterDataAccess : ForStateAssertion
            {
                internal static TriggeredAfterDataAccess Default { get; } = new TriggeredAfterDataAccess();

                internal TriggeredAfterDataAccess() { }
            }
        }
    }
}