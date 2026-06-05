using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public interface IRecordsBatch : IRecordsBatchSizeLimit
    {
        Type RecordType { get; }
        int SkippedRecords { get; }
        IReadOnlyList<Identity> Identities { get; }
    }

    public interface IRecordsBatch<out TRecord> : IRecordsBatch where TRecord : class
    {
        IReadOnlyList<TRecord> Records { get; }
    }

    public static class RecordsBatchExtensions
    {
        public static bool TryGetLastPresentRecordIdentity(this IRecordsBatch batch, out Identity identity)
        {
            identity = null;
            return batch.TryGetLastPresentRecordIndex(out int recordIndex)
                && batch.TryGetPresentRecordIdentity(recordIndex, out identity);
        }

        public static bool TryGetLastPresentIncludedRecord<TRecord>(this IRecordsBatch<TRecord> batch, out TRecord record)
            where TRecord : class
        {
            record = null;
            return batch.TryGetLastPresentRecordIndex(out int recordIndex)
                && batch.TryGetPresentIncludedRecord(recordIndex, out record);
        }

        public static bool TryGetLastPresentRecordIndex(this IRecordsBatch batch, out int recordIndex)
        {
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");
            for (recordIndex = batch.Identities.Count - 1; recordIndex >= 0; recordIndex--)
                if (batch.CheckRecordIsPresent(recordIndex))
                    return true;
            return false;
        }

        public static bool CheckHasSkippedMissingRecords<TRecord>(this IRecordsBatch<TRecord> batch)
            where TRecord : class
        {
            if (!batch.CheckRecordsWasIncluded()) return CheckHasSkippedMissingRecords((IRecordsBatch)batch);

            int skippedMissingRecords = 0;

            for (int recordIndex = 0; recordIndex < batch.Identities.Count; recordIndex++)
                if (!batch.CheckRecordIsPresent(recordIndex))
                    skippedMissingRecords++;

            if (skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords)
                throw new ApplicationException("skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords");

            return skippedMissingRecords > 0;
        }

        public static bool CheckHasSkippedMissingRecords(this IRecordsBatch batch)
        {
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");

            int skippedMissingRecords = 0;

            for (int recordIndex = 0; recordIndex < batch.Identities.Count; recordIndex++)
                if (!batch.CheckRecordIsPresent(recordIndex))
                    skippedMissingRecords++;

            if (skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords)
                throw new ApplicationException("skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords");

            return skippedMissingRecords > 0;
        }

        public static IEnumerable<Identity> EnumeratePresentIdentities(this IRecordsBatch batch)
        {
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");

            int skippedMissingRecords = 0;

            for (int recordIndex = 0; recordIndex < batch.Identities.Count; recordIndex++)
            {
                if (batch.TryGetPresentRecordIdentity(recordIndex, out var identity))
                {
                    yield return identity;
                }
                else
                {
                    skippedMissingRecords++;
                }
            }

            if (skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords)
                throw new ApplicationException("skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords");
        }

        public static IEnumerable<TRecord> EnumeratePresentIncludedRecords<TRecord>(this IRecordsBatch<TRecord> batch)
            where TRecord : class
        {
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");

            int skippedMissingRecords = 0;

            for (int recordIndex = 0; recordIndex < batch.Identities.Count; recordIndex++)
            {
                if (batch.TryGetPresentIncludedRecord(recordIndex, out var record))
                {
                    yield return record;
                }
                else
                {
                    skippedMissingRecords++;
                }
            }

            if (skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords)
                throw new ApplicationException("skippedMissingRecords > 0 && skippedMissingRecords != batch.SkippedRecords");
        }

        public static bool CheckRecordIsPresent<TRecord>(this IRecordsBatch<TRecord> batch, int recordIndex)
            where TRecord : class
            => batch.CheckRecordsWasIncluded()
                ? batch.TryGetPresentIncludedRecord(recordIndex, out _)
                : CheckRecordIsPresent((IRecordsBatch)batch, recordIndex);

        public static bool TryGetPresentIncludedRecord<TRecord>(this IRecordsBatch<TRecord> batch, int recordIndex, out TRecord record)
            where TRecord : class
        {
            bool recordIsPresent = CheckRecordIsPresent((IRecordsBatch)batch, recordIndex);
            record = batch.GetIncludedRecords()[recordIndex];
            bool recordIsIncluded = record != null;
            if (recordIsIncluded != recordIsPresent) throw new ApplicationException("recordIsIncluded != recordIsPresent");
            return recordIsIncluded;
        }

        public static IReadOnlyList<TRecord> GetIncludedRecords<TRecord>(this IRecordsBatch<TRecord> batch)
            where TRecord : class
        {
            if (!batch.CheckRecordsWasIncluded()) throw new ApplicationException("!batch.CheckRecordsWasIncluded()");
            return batch.Records ?? throw new ApplicationException("batch.Records == null");
        }

        public static bool CheckRecordsWasIncluded<TRecord>(this IRecordsBatch<TRecord> batch)
            where TRecord : class
        {
            bool included = batch.Records != null;
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");
            if (included && batch.Identities.Count != batch.Records.Count) throw new ApplicationException("included && batch.Identities.Count != batch.Records.Count");
            return included;
        }

        public static bool CheckRecordIsPresent(this IRecordsBatch batch, int recordIndex)
            => batch.TryGetPresentRecordIdentity(recordIndex, out _);

        public static bool TryGetPresentRecordIdentity(this IRecordsBatch batch, int recordIndex, out Identity identity)
        {
            if (batch.Identities == null) throw new ApplicationException("batch.Identities == null");
            if (recordIndex < 0 || recordIndex >= batch.Identities.Count) throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, message: null);
            identity = batch.Identities[recordIndex];
            if (identity == null && batch.SkippedRecords == 0) throw new ApplicationException("batch.Identities[recordIndex] == null && batch.SkippedRecords == 0");
            return identity != null;
        }
    }
}