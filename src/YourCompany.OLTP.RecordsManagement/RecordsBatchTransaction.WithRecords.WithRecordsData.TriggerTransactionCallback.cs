using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithRecords<TRecord>
        {
            public abstract partial class WithRecordsData<TRecordData>
                : RecordsBatchTransactionCallback.Sender.ITriggerTransactionCallback,
                TransactionCallback.Sender.IWithExtraInterfaces
            {
                private RecordsBatchTransactionCallback.ExtraInterfaceProvidersList _extraInterfaceProviders;

                public RecordsBatchTransactionCallback.CurrentlyTriggeringRecord TriggeringRecord
                    => _extraInterfaceProviders?.TriggeringRecord
                        ?? throw new ApplicationException("_extraInterfaceProviders == null");

                protected void TriggerTransactionCallbackBeforeDataChanging(int recordIndex, object possiblyEventArgs)
                {
                    if (possiblyEventArgs == null) throw new ArgumentNullException(nameof(possiblyEventArgs));
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsChangesAssertionStarted();

                    var builtRecord = GetBuiltRecord(recordIndex);
                    builtRecord.State.EnsureIsBeforeDataChanging();
                    if (builtRecord.BeforeDataChangingAssertionWasTriggered) throw new ApplicationException("builtRecord.BeforeDataChangingAssertionWasTriggered");
                    if (possiblyEventArgs is TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging)
                    {
                        builtRecord = builtRecord.WithBeforeDataChangingAssertionTriggered();
                        _builtRecords[recordIndex] = builtRecord;
                    }
                    else if (possiblyEventArgs is TransactionCallback.ForStateAssertion)
                    {
                        throw new ApplicationException("possiblyEventArgs is TransactionCallback.ForStateAssertion");
                    }

                    if (possiblyEventArgs is EventArgs eventArgs)
                        TriggerTransactionCallback(recordIndex, eventArgs);
                }

                protected virtual void TriggerTransactionCallback(int recordIndex, EventArgs eventArgs)
                {
                    if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsAfterConfiguration();
                    CurrentRunState.EnsureIsBeforeReturning();

                    var extraInterfaceProviders = GetExtraInterfaceProviders();
                    if (extraInterfaceProviders != null)
                    {
                        GetExtraInterfaceProviders().TriggerTransactionCallback(recordIndex, eventArgs);
                    }
                    else
                    {
                        GetBuiltRecord(recordIndex).TransactionCallback(this, eventArgs);
                    }
                }

                protected RecordsBatchTransactionCallback.ExtraInterfaceProvidersList GetExtraInterfaceProviders()
                {
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsAfterConfiguration();
                    CurrentRunState.EnsureIsBeforeReturning();
                    return _extraInterfaceProviders ?? (_extraInterfaceProviders = CollectExtraInterfaceProviders());
                }

                protected virtual RecordsBatchTransactionCallback.ExtraInterfaceProvidersList CollectExtraInterfaceProviders()
                {
                    var extraInterfaceProviders = new RecordsBatchTransactionCallback.ExtraInterfaceProvidersList(this);
                    if (this is RecordsBatchTransactionCallback.IExtraInterfacesProvider extraInterfacesProvider)
                        extraInterfaceProviders.Add(extraInterfacesProvider);
                    return extraInterfaceProviders;
                }

                void RecordsBatchTransactionCallback.Sender.ITriggerTransactionCallback.TriggerTransactionCallback(
                    int recordIndex, EventArgs eventArgs)
                {
                    if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                    if (_extraInterfaceProviders == null) throw new ApplicationException("_extraInterfaceProviders == null");
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsAfterConfiguration();
                    CurrentRunState.EnsureIsBeforeReturning();

                    if (TriggeringRecord.RecordIndex != recordIndex)
                        throw new ApplicationException("TriggeringRecord.RecordIndex != recordIndex");
                    if (TriggeringRecord.FirstTriggeredEventArgs == null)
                        throw new ApplicationException("TriggeringRecord.FirstTriggeredEventArgs == null");

                    var builtRecord = GetBuiltRecord(recordIndex);
                    builtRecord.TransactionCallback(this, eventArgs);
                }

                bool TransactionCallback.Sender.IWithExtraInterfaces.TryGetExtraInterfaceImplementor(
                    Type type, out object implementor)
                {
                    if (_extraInterfaceProviders == null) throw new ApplicationException("_extraInterfaceProviders == null");
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsAfterConfiguration();
                    CurrentRunState.EnsureIsBeforeReturning();
                    return _extraInterfaceProviders.TryGetExtraInterfaceImplementor(type, out implementor);
                }
            }
        }
    }
}