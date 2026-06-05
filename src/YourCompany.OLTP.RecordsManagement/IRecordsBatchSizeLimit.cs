namespace YourCompany.OLTP.RecordsManagement
{
    public interface IRecordsBatchSizeLimit
    {
        int MaxRecordsInBatch { get; }
    }
}