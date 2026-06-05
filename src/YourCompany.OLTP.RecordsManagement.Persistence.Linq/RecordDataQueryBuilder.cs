using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq
{
    public abstract class RecordDataQueryBuilder
    {
        public abstract Type RecordDataType { get; }
        public abstract bool CheckCanHandleSpecification(object specification);
        private RecordDataQueryBuilder() { }

        public abstract class ForSingleRecordData<TRecordData> : RecordDataQueryBuilder where TRecordData : class
        {
            public sealed override Type RecordDataType => typeof(TRecordData);

            public virtual Expression<Func<TQueryableRecordData, bool>> Filter<TQueryableRecordData>()
                where TQueryableRecordData : class, TRecordData => null;

            public virtual Expression<Func<TQueryableRecordData, bool>> Filter<TQueryableRecordData>(object specification)
                where TQueryableRecordData : class, TRecordData => null;

            public virtual void ChangeToMatch<TQueryableRecordData>(TQueryableRecordData recordData, object specification)
                where TQueryableRecordData : class, TRecordData
            { }
        }

        public sealed class ByRecordDataType : Dictionary<Type, IReadOnlyList<RecordDataQueryBuilder>>,
            IRecordDataQueryBuildersProvider
        {
            public ByRecordDataType() { }
            public ByRecordDataType(IDictionary<Type, IReadOnlyList<RecordDataQueryBuilder>> dictionary) : base(dictionary) { }

            public ByRecordDataType(IEnumerable<RecordDataQueryBuilder> queryBuilders)
            {
                if (queryBuilders == null) throw new ArgumentNullException(nameof(queryBuilders));
                foreach (var g in queryBuilders.GroupBy(b => b.RecordDataType)) Add(g.Key, g.ToList());
            }

            IReadOnlyList<RecordDataQueryBuilder> IRecordDataQueryBuildersProvider.GetQueryBuilders(Type recordDataType)
            {
                if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));
                TryGetValue(recordDataType, out var queryBuilders);
                return queryBuilders ?? Array.Empty<RecordDataQueryBuilder>();
            }
        }
    }
}