using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.OLTP.StateOwnership.Reflection.DI;
using YourCompany.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal class ScopedRecordsBatchTransactionFactory
    {
        private static readonly Expression<Action<ScopedRecordsBatchTransactionFactory>>
            CreateTransactionMethod = provider => provider.CreateTransaction<object, object>(null);

        private static readonly MethodInfo CreateTransactionMethodInfo
            = GetMemberHelper.GetMethodInfo(CreateTransactionMethod).GetGenericMethodDefinition();

        protected IServiceProvider Provider { get; }

        public ScopedRecordsBatchTransactionFactory(IServiceProvider provider) => Provider = provider ?? throw new ArgumentNullException(nameof(provider));

        internal RecordTypeInfo GetRecordTypeInfo(Type recordType)
        {
            if (recordType == null) throw new ArgumentNullException(nameof(recordType));
            var recordTypesMap = Provider.GetRequiredService<RecordTypesMap>();
            if (!recordTypesMap.TryGetValue(recordType, out RecordTypeInfo recordTypeInfo))
                throw new ApplicationException("!recordTypesMap.TryGetValue(recordType, out RecordTypeInfo recordTypeInfo)");
            if (recordTypeInfo.Type != recordType) throw new ApplicationException("recordTypeInfo.Type != recordType");
            return recordTypeInfo;
        }

        internal RecordsBatchTransaction.WithRecords<TRecord> CreateTransaction<TRecord>(
            RecordTypeInfo recordTypeInfo, RecordsDataAccess.IStarting recordsDataAccess)
            where TRecord : class
            => (RecordsBatchTransaction.WithRecords<TRecord>)CreateTransaction(recordTypeInfo, recordsDataAccess);

        internal object CreateTransaction(RecordTypeInfo recordTypeInfo, RecordsDataAccess.IStarting recordsDataAccess)
        {
            if (recordTypeInfo == null) throw new ArgumentNullException(nameof(recordTypeInfo));
            if (recordsDataAccess == null) throw new ArgumentNullException(nameof(recordsDataAccess));
            if (!recordTypeInfo.IsRecord) throw new ApplicationException("!recordTypeInfo.IsRecord");
            return CreateTransactionMethodInfo
                .MakeGenericMethod(recordTypeInfo.Type, recordTypeInfo.RecordDataType)
                .Invoke(this, new object[] { recordsDataAccess });
        }

        internal virtual RecordsBatchTransaction.WithRecords<TRecord> CreateTransaction<TRecord, TRecordData>(
            RecordsDataAccess.IStarting recordsDataAccess)
            where TRecord : class
            where TRecordData : class
            => new RecordsBatchTransaction<TRecord, TRecordData>.CurrentStateAccess.Provider(this, recordsDataAccess);

        internal ScopedUseCasesProvider GetUseCases() => Provider.GetRequiredService<ScopedUseCasesProvider>();
        
        internal TRecord ResolveRecord<TRecord>()
            where TRecord : class 
            => Provider.GetRequiredService<ScopedRecordsProvider>().ResolveRecord<TRecord>();
    }
}