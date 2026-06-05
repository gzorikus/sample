using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                private ComposingRecordsDataAccessProxy _recordsDataAccessProxy;
                private Type _replacedCurrentlyHandledEntityRecordType;

                internal RepositoryRecords(
                    DI.ScopedRecordsBatchTransactionFactory provider, ComposingRecordsDataAccessProxy recordsDataAccess)
                    : base(provider, recordsDataAccess) => _recordsDataAccessProxy = recordsDataAccess;

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
                    if (_recordsDataAccessProxy == null) throw new ApplicationException("_recordsDataAccessProxy == null");
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
                    if (_recordsDataAccessProxy == null) throw new ApplicationException("_recordsDataAccessProxy == null");
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

                    _replacedCurrentlyHandledEntityRecordType = ScopedRecordsBatchTransactionFactory.ReplaceCurrentlyHandledEntityRecordType(
                        _recordsDataAccessProxy.EntityRecordTypeInfo.Type);

                    if (CheckToRead()) _recordsDataAccessProxy.PrepareToRead();
                    if (!ReadOnly) _recordsDataAccessProxy.PrepareToPersist();
                    if (!ReadOnly || ReadOnlyIncludeRecords) _recordsDataAccessProxy.PrepareToFinish();
                }

                protected override void TriggerUseCaseParameters()
                {
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsChangesAssertionStarted();
                    UseCurrentRecordTypeAndIncludeForChanges();
                    base.TriggerUseCaseParameters();
                }

                protected override async Task Return(CancellationToken cancellationToken, Exception runException = null)
                {
                    var recordsDataAccessProxy = _recordsDataAccessProxy ?? throw new ApplicationException("_recordsDataAccessProxy == null");
                    _recordsDataAccessProxy = null;

                    try
                    {
                        await base.Return(cancellationToken, runException);
                    }
                    finally
                    {
                        if (_replacedCurrentlyHandledEntityRecordType != null)
                            ScopedRecordsBatchTransactionFactory.ReplaceCurrentlyHandledEntityRecordType(_replacedCurrentlyHandledEntityRecordType);

                        recordsDataAccessProxy.EnsureNoPendingTasks();
                    }
                }

                private IReadOnlyList<IConfiguredIdentically> GetIdenticallyConfiguredTransactions()
                {
                    if (_recordsDataAccessProxy == null) throw new ApplicationException("_recordsDataAccessProxy == null");
                    return (IReadOnlyList<IConfiguredIdentically>)_recordsDataAccessProxy.ComposedTransactions;
                }
            }
        }
    }
}