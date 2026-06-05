namespace YourCompany.OLTP.StateOwnership
{
    public interface ISpecification<in TRecordData> where TRecordData : class
    {
        bool Match(TRecordData recordData);
    }
}