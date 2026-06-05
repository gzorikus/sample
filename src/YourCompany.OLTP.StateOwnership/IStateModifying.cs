using System.Collections.Generic;

namespace YourCompany.OLTP.StateOwnership
{
    public interface IStateModifying<TRecordData> where TRecordData : class
    {
        IReadOnlyList<ISpecification<TRecordData>> MatchingWithoutDataChanges { get; }
        IReadOnlyList<ISpecification<TRecordData>> MatchingAfterDataChanging { get; }
        TRecordData SettingDataProperties { get; }
        TRecordData LockedRecordData { get; }
        void MatchBeforeDataChanging(ISpecification<TRecordData> specification);
        void ChangeDataToMatch(ISpecification<TRecordData> specification);
    }
}