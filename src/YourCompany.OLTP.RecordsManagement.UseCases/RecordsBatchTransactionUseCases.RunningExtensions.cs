using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Threading;

namespace YourCompany.OLTP.RecordsManagement.UseCases
{
    public static partial class RecordsBatchTransactionUseCases
    {
        public static async Task AuthorizeToSpecifiedTransaction<TRecordData>(
            this IEnumerable<IAuthorizer<TRecordData>> authorizers,
            IReadOnlyList<RecordsBatchTransactionSpecification> transactionSpecifications,
            IAuthorizerSpecificationsCollector<TRecordData> authorizingSpecificationsCollector,
            CancellationToken cancellationToken)
            where TRecordData : class
        {
            if (transactionSpecifications == null) throw new ArgumentNullException(nameof(transactionSpecifications));
            if (authorizingSpecificationsCollector == null) throw new ArgumentNullException(nameof(authorizingSpecificationsCollector));
            foreach (var authorizer in authorizers)
                await authorizer.AuthorizeToSpecifiedTransaction(
                    transactionSpecifications, authorizingSpecificationsCollector, cancellationToken);
        }

        public static async Task AuthorizeToSpecifiedTransaction<TRecordData>(
            this IEnumerable<IAuthorizer<TRecordData>> authorizers,
            IReadOnlyList<RecordsBatchTransactionSpecification> transactionSpecifications,
            IAuthorizerSpecificationsCollector<TRecordData> authorizingSpecificationsCollector,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken)
            where TRecordData : class
        {
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            if (awaitTasksList.Count > 0) throw new ApplicationException("awaitTasksList.Count > 0");

            try
            {
                authorizers.AddAuthorizeToSpecifiedTransactionTasks(
                    transactionSpecifications, authorizingSpecificationsCollector, awaitTasksList, cancellationToken);
            }
            finally
            {
                await awaitTasksList.WaitOnce();
            }
        }

        public static void AddAuthorizeToSpecifiedTransactionTasks<TRecordData>(
            this IEnumerable<IAuthorizer<TRecordData>> authorizers,
            IReadOnlyList<RecordsBatchTransactionSpecification> transactionSpecifications,
            IAuthorizerSpecificationsCollector<TRecordData> authorizingSpecificationsCollector,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken) where TRecordData : class
        {
            if (transactionSpecifications == null) throw new ArgumentNullException(nameof(transactionSpecifications));
            if (authorizingSpecificationsCollector == null) throw new ArgumentNullException(nameof(authorizingSpecificationsCollector));
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            foreach (var authorizer in authorizers)
                awaitTasksList.Add(authorizer.AuthorizeToSpecifiedTransaction(
                    transactionSpecifications, authorizingSpecificationsCollector, cancellationToken));
        }

        public static void PrepareRecordsBatchBeforeDataChangingAssertionsWithoutRecords(
            this IEnumerable<IChangesPreparer> preparers, IRecordsBatch recordsBatch)
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            foreach (var preparer in preparers)
                preparer.PrepareRecordsBatchBeforeDataChangingAssertions(recordsBatch);
        }

        public static void PrepareRecordsBatchBeforeDataChangingAssertionsWithoutRecords<TRecord>(
            this IEnumerable<IChangesPreparer<TRecord>> preparers, IRecordsBatch<TRecord> recordsBatch)
            where TRecord : class
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            foreach (var preparer in preparers)
                preparer.PrepareRecordsBatchBeforeDataChangingAssertions(recordsBatch);
        }

        public static async Task HandleRecordsBatchAfterDataAccessTransactionCallbacks(
            this IEnumerable<IFinishingHandler> handlers,
            IRecordsBatch recordsBatch,
            CancellationToken cancellationToken)
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            foreach (var handler in handlers)
                await handler.HandleRecordsBatchAfterDataAccessTransactionCallbacks(recordsBatch, cancellationToken);
        }

        public static async Task HandleRecordsBatchAfterDataAccessTransactionCallbacks(
            this IEnumerable<IFinishingHandler> handlers,
            IRecordsBatch recordsBatch,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken)
        {
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            if (awaitTasksList.Count > 0) throw new ApplicationException("awaitTasksList.Count > 0");

            try
            {
                handlers.AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks(
                    recordsBatch, awaitTasksList, cancellationToken);
            }
            finally
            {
                await awaitTasksList.WaitOnce();
            }
        }

        public static void AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks(
            this IEnumerable<IFinishingHandler> handlers,
            IRecordsBatch recordsBatch,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken)
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            foreach (var handler in handlers)
                awaitTasksList.Add(handler.HandleRecordsBatchAfterDataAccessTransactionCallbacks(
                    recordsBatch, cancellationToken));
        }

        public static async Task HandleRecordsBatchAfterDataAccessTransactionCallbacks<TRecord>(
            this IEnumerable<IFinishingHandler<TRecord>> handlers,
            IRecordsBatch<TRecord> recordsBatch,
            CancellationToken cancellationToken)
            where TRecord : class
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            foreach (var handler in handlers)
                await handler.HandleRecordsBatchAfterDataAccessTransactionCallbacks(recordsBatch, cancellationToken);
        }

        public static async Task HandleRecordsBatchAfterDataAccessTransactionCallbacks<TRecord>(
            this IEnumerable<IFinishingHandler<TRecord>> handlers,
            IRecordsBatch<TRecord> recordsBatch,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken)
            where TRecord : class
        {
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            if (awaitTasksList.Count > 0) throw new ApplicationException("awaitTasksList.Count > 0");

            try
            {
                handlers.AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks(
                    recordsBatch, awaitTasksList, cancellationToken);
            }
            finally
            {
                await awaitTasksList.WaitOnce();
            }
        }

        public static void AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks<TRecord>(
            this IEnumerable<IFinishingHandler<TRecord>> handlers,
            IRecordsBatch<TRecord> recordsBatch,
            AwaitTasksList awaitTasksList,
            CancellationToken cancellationToken)
            where TRecord : class
        {
            if (recordsBatch == null) throw new ArgumentNullException(nameof(recordsBatch));
            if (awaitTasksList == null) throw new ArgumentNullException(nameof(awaitTasksList));
            foreach (var handler in handlers)
                awaitTasksList.Add(handler.HandleRecordsBatchAfterDataAccessTransactionCallbacks(recordsBatch, cancellationToken));
        }
    }
}