using System;
using System.Collections.Generic;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal static partial class ComposableRecordsBatchTransaction
    {
        internal abstract partial class ConfiguredIdentically<TRecord, TRecordData>
        {
            internal abstract class RepositoryRecords : ConfiguredIdentically<TRecord, TRecordData>,
                IConfiguredIdentically
            {
                internal RepositoryRecords(
                    ScopedRecordsBatchTransactionFactory provider, RecordsDataAccess.IStarting recordsDataAccess)
                    : base(provider, recordsDataAccess) { }

                bool IConfiguredIdentically.Add(RecordsBatchTransactionSpecification specification)
                    => throw new ApplicationException(nameof(RepositoryRecords));

                bool IConfiguredIdentically.AddSupportedRecordDataSpecification(
                    RecordsBatchTransactionSpecification specification)
                    => throw new ApplicationException(nameof(RepositoryRecords));

                void IConfiguredIdentically.FinishConfiguration(bool includeForChanges)
                    => throw new ApplicationException(nameof(RepositoryRecords));

                protected override bool Handle(RecordsBatchTransactionSpecification specification)
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    CurrentRunState.EnsureIsConfiguration();

                    bool thisFound = false;
                    bool anyHandled = false;

                    var composedTransactions = GetIdenticallyConfiguredTransactions() ?? throw new ApplicationException("GetIdenticallyConfiguredTransactions() == null");
                    for (int i = 0; i < composedTransactions.Count; i++)
                    {
                        var composedTransaction = composedTransactions[i] ?? throw new ApplicationException("composedTransaction == null");
                        if (composedTransaction == this)
                        {
                            if (thisFound) throw new ApplicationException("thisFound");
                            thisFound = true;

                            var specificationToAdd = specification is RecordsBatchTransactionSpecification.ISpecificationWrapper
                                ? GetSupportedRecordDataSpecificationToAdd(specification)
                                : specification;

                            bool handled = specificationToAdd == null || base.Handle(specificationToAdd);
                            anyHandled = anyHandled || handled;
                        }
                        else
                        {
                            if (specification is RecordsBatchTransactionSpecification.ReadOnly.IncludeRecords
                                && !CheckToIncludeNonRepositoryRecords(composedTransaction.RecordTypeInfo))
                                continue;

                            bool added = specification is RecordsBatchTransactionSpecification.ISpecificationWrapper
                                ? composedTransaction.AddSupportedRecordDataSpecification(specification)
                                : composedTransaction.Add(specification);

                            anyHandled = anyHandled || added;
                        }
                    }

                    if (!thisFound) throw new ApplicationException("!thisFound");
                    return anyHandled;
                }

                protected virtual bool CheckToIncludeNonRepositoryRecords(ComposableRecordTypeInfo recordTypeInfo) => false;

                protected override void FinishConfiguration()
                {
                    CurrentRunState.EnsureIsConfiguration();

                    bool thisFound = false;

                    var composedTransactions = GetIdenticallyConfiguredTransactions() ?? throw new ApplicationException("GetIdenticallyConfiguredTransactions() == null");
                    for (int i = 0; i < composedTransactions.Count; i++)
                    {
                        var composedTransaction = composedTransactions[i] ?? throw new ApplicationException("composedTransaction == null");
                        if (composedTransaction == this)
                        {
                            if (thisFound) throw new ApplicationException("thisFound");
                            thisFound = true;
                            base.FinishConfiguration();
                            if (!ReadOnly) UseCurrentRecordTypeAndIncludeForChanges();
                        }
                        else
                        {
                            bool includeForChanges = !ReadOnly && CheckToIncludeNonRepositoryRecords(
                                composedTransaction.RecordTypeInfo);
                            composedTransaction.FinishConfiguration(includeForChanges);
                        }
                    }

                    if (!thisFound) throw new ApplicationException("!thisFound");
                }

                protected override void TriggerUseCaseParameters()
                {
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsChangesAssertionStarted();
                    UseCurrentRecordTypeAndIncludeForChanges();
                    base.TriggerUseCaseParameters();
                }

                protected sealed override IReadOnlyList<IComposingRecords> GetRecordsComposingTransactions()
                    => GetIdenticallyConfiguredTransactions();

                protected abstract IReadOnlyList<IConfiguredIdentically> GetIdenticallyConfiguredTransactions();
            }
        }
    }
}