using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static class RecordsBatchTransactionCallback
    {
        internal interface IExtraInterfacesProvider
        {
            void CollectTransactionCallbackExtraInterfaces(
                object sender, IExtraInterfacesCollector extraInterfacesCollector);
        }

        internal interface IExtraInterfacesCollector
        {
            void CollectExtraInterface(Type type, object implementor);
        }

        internal static class Sender
        {
            internal interface ITriggerTransactionCallback
            {
                void TriggerTransactionCallback(int recordIndex, EventArgs eventArgs);
            }
        }

        internal class ExtraInterfaceProvidersList : List<IExtraInterfacesProvider>,
            IExtraInterfacesCollector,
            Sender.ITriggerTransactionCallback,
            TransactionCallback.Sender.IWithExtraInterfaces
        {
            private readonly Sender.ITriggerTransactionCallback _sender;
            private Dictionary<Type, object> _extraInterfaces;
            private bool _extraInterfacesCollecting;
            private CurrentlyTriggeringRecord? _triggeringRecord;

            internal CurrentlyTriggeringRecord TriggeringRecord => _triggeringRecord
                ?? throw new ApplicationException("!_triggeringRecord.HasValue");

            internal ExtraInterfaceProvidersList(Sender.ITriggerTransactionCallback sender)
                => _sender = sender ?? throw new ArgumentNullException(nameof(sender));

            internal ExtraInterfaceProvidersList(
                Sender.ITriggerTransactionCallback sender, IEnumerable<IExtraInterfacesProvider> collection)
                : base(collection)
                => _sender = sender ?? throw new ArgumentNullException(nameof(sender));

            internal ExtraInterfaceProvidersList(Sender.ITriggerTransactionCallback sender, int capacity)
                : base(capacity)
                => _sender = sender ?? throw new ArgumentNullException(nameof(sender));

            public void TriggerTransactionCallback(int recordIndex, EventArgs eventArgs)
            {
                if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

                var firstTriggeringRecord = _triggeringRecord;

                try
                {
                    if (firstTriggeringRecord.HasValue)
                    {
                        if (firstTriggeringRecord.Value.RecordIndex != recordIndex)
                            throw new ApplicationException("firstTriggeringRecord.Value.RecordIndex != recordIndex");
                        if (firstTriggeringRecord.Value.FirstTriggeredEventArgs == null)
                            throw new ApplicationException("firstTriggeringRecord.Value.FirstTriggeredEventArgs == null");
                        if (firstTriggeringRecord.Value.FirstTriggeredEventArgs == eventArgs)
                            throw new ApplicationException("firstTriggeringRecord.Value.FirstTriggeredEventArgs == eventArgs");
                    }
                    else
                    {
                        _triggeringRecord = new CurrentlyTriggeringRecord(recordIndex, eventArgs);
                        CollectTransactionCallbackExtraInterfaces();
                    }

                    _sender.TriggerTransactionCallback(recordIndex, eventArgs);
                }
                finally
                {
                    _triggeringRecord = firstTriggeringRecord;
                    if (!firstTriggeringRecord.HasValue) _extraInterfaces?.Clear();
                }
            }

            internal void CollectTransactionCallbackExtraInterfaces()
            {
                if (_extraInterfacesCollecting) throw new ApplicationException("_extraInterfacesCollecting");
                _extraInterfacesCollecting = true;
                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        var provider = this[i] ?? throw new ApplicationException("provider == null");
                        provider.CollectTransactionCallbackExtraInterfaces(_sender, extraInterfacesCollector: this);
                    }
                }
                finally
                {
                    _extraInterfacesCollecting = false;
                }
            }

            void IExtraInterfacesCollector.CollectExtraInterface(Type type, object implementor)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (implementor == null) throw new ArgumentNullException(nameof(implementor));
                if (!type.IsAssignableFrom(implementor.GetType())) throw new ApplicationException("!type.IsAssignableFrom(implementor.GetType())");
                if (!_extraInterfacesCollecting) throw new ApplicationException("_extraInterfacesCollecting");
                _extraInterfaces = _extraInterfaces ?? new Dictionary<Type, object>();
                _extraInterfaces.Add(type, implementor);
            }

            public bool TryGetExtraInterfaceImplementor(Type type, out object implementor)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (_extraInterfacesCollecting) throw new ApplicationException("_extraInterfacesCollecting");
                implementor = null;
                return _extraInterfaces != null && _extraInterfaces.TryGetValue(type, out implementor);
            }
        }

        internal readonly struct CurrentlyTriggeringRecord
        {
            internal int RecordIndex { get; }
            internal EventArgs FirstTriggeredEventArgs { get; }

            internal CurrentlyTriggeringRecord(int recordIndex, EventArgs firstTriggeredEventArgs)
            {
                if (recordIndex < 0) throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, message: null);
                RecordIndex = recordIndex;
                FirstTriggeredEventArgs = firstTriggeredEventArgs ?? throw new ArgumentNullException(nameof(firstTriggeredEventArgs));
            }
        }
    }
}