using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.UseCases
{
    public static partial class RecordsBatchTransactionUseCases
    {
        public interface IAuthorizer<in TRecordData> where TRecordData : class
        {
            Task AuthorizeToSpecifiedTransaction(
                IReadOnlyList<RecordsBatchTransactionSpecification> transactionSpecifications,
                IAuthorizerSpecificationsCollector<TRecordData> authorizingSpecificationsCollector,
                CancellationToken cancellationToken);
        }

        public interface IAuthorizerSpecificationsCollector<out TRecordData> where TRecordData : class
        {
            void Collect(ISpecification<TRecordData> authorizingSpecification);
        }

        public interface IChangesPreparer
        {
            void PrepareRecordsBatchBeforeDataChangingAssertions(IRecordsBatch recordsBatch);
        }

        public interface IChangesPreparer<in TRecord> where TRecord : class
        {
            void PrepareRecordsBatchBeforeDataChangingAssertions(IRecordsBatch<TRecord> recordsBatch);
        }

        public interface IFinishingHandler
        {
            Task HandleRecordsBatchAfterDataAccessTransactionCallbacks(
                IRecordsBatch recordsBatch, CancellationToken cancellationToken);
        }

        public interface IFinishingHandler<in TRecord> where TRecord : class
        {
            Task HandleRecordsBatchAfterDataAccessTransactionCallbacks(
                IRecordsBatch<TRecord> recordsBatch, CancellationToken cancellationToken);
        }
    }
}