using System;
using System.Collections.Generic;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership;
using YourCompany.OLTP.StateOwnership.TransactionalComposition;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal static partial class ComposableRecordsBatchTransaction
    {
        internal abstract class ComposingRecords<TRecord, TRecordData>
            : RecordsBatchTransaction<TRecord, TRecordData>.CurrentStateAccess.Provider,
            IComposingRecords,
            RecordsBatchTransactionCallback.IExtraInterfacesProvider,
            TransactionalCompositionTransactionCallback.ITriggeringSenderExtraInterface
            where TRecord : class
            where TRecordData : class
        {
            ComposableRecordTypeInfo IComposingRecords.RecordTypeInfo => RecordTypeInfo ?? throw new ApplicationException("RecordTypeInfo == null");
            protected abstract ComposableRecordTypeInfo RecordTypeInfo { get; }

            internal ComposingRecords(
                ScopedRecordsBatchTransactionFactory provider, RecordsDataAccess.IStarting recordsDataAccess)
                : base(provider, recordsDataAccess) { }

            void RecordsBatchTransactionCallback.IExtraInterfacesProvider.CollectTransactionCallbackExtraInterfaces(
                object sender, RecordsBatchTransactionCallback.IExtraInterfacesCollector extraInterfacesCollector)
            {
                if (!ReferenceEquals(sender, this)) throw new ApplicationException("!ReferenceEquals(sender, this)");
                CollectTransactionCallbackExtraInterfaces(extraInterfacesCollector);
            }

            protected virtual void CollectTransactionCallbackExtraInterfaces(
                RecordsBatchTransactionCallback.IExtraInterfacesCollector extraInterfacesCollector)
            {
                if (extraInterfacesCollector == null) throw new ArgumentNullException(nameof(extraInterfacesCollector));
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsAfterConfiguration();
                CurrentRunState.EnsureIsBeforeReturning();
                if (CurrentRunState == State.ChangesAssertionStarted)
                    extraInterfacesCollector.CollectExtraInterface(
                        typeof(TransactionalCompositionTransactionCallback.ITriggeringSenderExtraInterface), this);
            }

            void TransactionalCompositionTransactionCallback.ITriggeringSenderExtraInterface.Trigger(
                EventArgs parametersToJoinTransactionWith, object atComposedRecord)
            {
                if (parametersToJoinTransactionWith == null) throw new ArgumentNullException(nameof(parametersToJoinTransactionWith));
                if (atComposedRecord == null) throw new ArgumentNullException(nameof(atComposedRecord));
                if (parametersToJoinTransactionWith is TransactionCallback.ForStateAssertion) throw new ApplicationException("parametersToJoinTransactionWith is TransactionCallback.ForStateAssertion)");
                if (atComposedRecord is TRecord) throw new ApplicationException("atComposedRecord is TRecord");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsChangesAssertionStarted();

                bool thisFound = false;
                bool handledBySingleTransaction = false;

                var composedTransactions = GetRecordsComposingTransactions() ?? throw new ApplicationException("GetRecordsComposingTransactions()");
                for (int i = 0; i < composedTransactions.Count; i++)
                {
                    var composedTransaction = composedTransactions[i] ?? throw new ApplicationException("composedTransaction == null");
                    if (composedTransaction == this)
                    {
                        if (thisFound) throw new ApplicationException("thisFound");
                        thisFound = true;
                    }
                    else
                    {
                        if (composedTransaction.HandleTriggering(parametersToJoinTransactionWith, atComposedRecord))
                        {
                            if (handledBySingleTransaction) throw new ApplicationException("handledBySingleTransaction");
                            handledBySingleTransaction = true;
                        }
                    }
                }

                if (!thisFound) throw new ApplicationException("!thisFound");
                if (!handledBySingleTransaction) throw new ApplicationException("!handledBySingleTransaction");
                UseCurrentRecordTypeAndIncludeForChanges();
            }

            bool IComposingRecords.HandleTriggering(
                EventArgs parametersToJoinTransactionWith, object atComposedRecord)
                => HandleTriggering(parametersToJoinTransactionWith, atComposedRecord);

            protected virtual bool HandleTriggering(EventArgs parametersToJoinTransactionWith, object atComposedRecord)
            {
                if (parametersToJoinTransactionWith == null) throw new ArgumentNullException(nameof(parametersToJoinTransactionWith));
                if (atComposedRecord == null) throw new ArgumentNullException(nameof(atComposedRecord));
                if (parametersToJoinTransactionWith is TransactionCallback.ForStateAssertion) throw new ApplicationException("parametersToJoinTransactionWith is TransactionCallback.ForStateAssertion)");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                if (Records == null) throw new ApplicationException("Records == null");
                if (Identities.Count != Records.Count) throw new ApplicationException("Identities.Count != Records.Count");
                CurrentRunState.EnsureIsChangesAssertionStarted();

                if (atComposedRecord is not TRecord) return false;

                for (int recordIndex = 0; recordIndex < Records.Count; recordIndex++)
                {
                    if (!this.CheckRecordIsPresent(recordIndex)) continue;
                    var builtRecord = GetBuiltRecord(recordIndex);
                    if (builtRecord.Record == atComposedRecord)
                    {
                        UseCurrentRecordTypeAndIncludeForChanges();
                        if (!HandleTriggeringByAnotherComposedRecord(recordIndex, builtRecord, parametersToJoinTransactionWith))
                            throw new ApplicationException("!HandleTriggeringByAnotherComposedRecord(recordIndex, builtRecord, parametersToJoinTransactionWith)");
                        return true;
                    }
                }

                throw new ApplicationException("!Records.Contains(atComposedRecord)");
            }

            protected virtual bool HandleTriggeringByAnotherComposedRecord(
                int recordIndex, BuiltRecord builtRecord, EventArgs eventArgs)
            {
                if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsChangesAssertionStarted();

                if (eventArgs is RecordsBatchTransactionSpecification.ReadOnlyIncompatible.ISpecificationWrapper)
                {
                    if (eventArgs is not ISpecification<TRecordData> modifyingSpecification)
                        throw new ApplicationException("eventArgs is not ISpecification<TRecordData> modifyingSpecification");
                    builtRecord.State.ChangeDataToMatch(modifyingSpecification);
                }
                else if (eventArgs is RecordsBatchTransactionSpecification.ISpecificationWrapper)
                {
                    if (eventArgs is not ISpecification<TRecordData> specification)
                        throw new ApplicationException("eventArgs is not ISpecification<TRecordData> specification");
                    builtRecord.State.MatchBeforeDataChanging(specification);
                }
                else
                {
                    TriggerTransactionCallback(recordIndex, eventArgs);
                }

                return true;
            }

            protected abstract void UseCurrentRecordType();
            protected abstract void UseCurrentRecordTypeAndIncludeForChanges();
            protected abstract void ResetCurrentRecordType();
            protected abstract IReadOnlyList<IComposingRecords> GetRecordsComposingTransactions();
        }
    }
}