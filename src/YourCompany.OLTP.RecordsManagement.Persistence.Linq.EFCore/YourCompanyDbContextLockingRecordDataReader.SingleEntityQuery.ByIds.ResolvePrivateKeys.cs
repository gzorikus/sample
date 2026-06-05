using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Configuration.EFCore.CollationAwareSorting;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.CollationAwareSorting;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public abstract partial class YourCompanyDbContextLockingRecordDataReader<TConfiguration>
    {
        public abstract partial class SingleEntityQuery<TRecordData, TQueryableRecordData>
        {
            public abstract partial class ByIds
            {
                protected override async Task<IReadOnlyList<long>> ResolvePrimaryKeys(
                    int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
                {
                    if (batchSize != PrimaryKeys.Count) throw new ApplicationException("batchSize != PrimaryKeys.Count");
                    if (recordsCountToSkip != 0) throw new ApplicationException("recordsCountToSkip != 0");
                    EnsureReadingForReadOnly();
                    if (_primaryKeysWithRecordDataByPrivateKey != null) throw new ApplicationException("_primaryKeysWithRecordDataByPrivateKey != null");

                    await ResolvePrivateKeys(cancellationToken);

                    List<long> existingRecordPrivateKeys = null;

                    for (int recordIndex = 0; recordIndex < PrimaryKeys.Count; recordIndex++)
                    {
                        var primaryKey = PrimaryKeys[recordIndex];
                        if (CheckToSkipPrimaryKeyAfterResolvingWithExistingRecordsData(recordIndex, primaryKey)) continue;
                        if (!primaryKey.CheckPrivateKeyIsAssigned()) throw new ApplicationException("!primaryKey.CheckPrivateKeyIsAssigned()");
                        long privateKey = primaryKey.PrivateKey ?? throw new ApplicationException("!primaryKey.PrivateKey.HasValue");
                        existingRecordPrivateKeys ??= new List<long>(PrimaryKeys.Count);
                        existingRecordPrivateKeys.Add(privateKey);
                    }

                    return existingRecordPrivateKeys;
                }

                protected virtual async Task ResolvePrivateKeys(CancellationToken cancellationToken)
                {
                    EnsureReadingForReadOnly();
                    if (_primaryKeysWithRecordDataByPrivateKey != null) throw new ApplicationException("_primaryKeysWithRecordDataByPrivateKey != null");

                    Dictionary<SortingKey, int> resolvePrivateKeysFromNatural = null;
                    Dictionary<Guid, int> resolvePrivateKeysFromPublic = null;

                    for (int recordIndex = 0; recordIndex < PrimaryKeys.Count; recordIndex++)
                    {
                        var primaryKey = PrimaryKeys[recordIndex] ?? throw new ApplicationException("PrimaryKeys[recordIndex] == null");
                        if (primaryKey.CheckPrivateKeyIsAssigned()) continue;

                        var withoutExtraValues = primaryKey.AsWithoutExtraValues() ?? throw new ApplicationException("withoutExtraValues == null");
                        var naturalPublicCursor = withoutExtraValues
                            .AsOriginalBeforeSortingImplementedWith<CollationAwareSortingUniqueKeyAdapter>();

                        bool privatePrimaryKeyCanBeResolved = false;

                        if (naturalPublicCursor != null && naturalPublicCursor.CanBeResolvedIntoPrivatePrimaryKey)
                        {
                            resolvePrivateKeysFromNatural ??= new Dictionary<SortingKey, int>(PrimaryKeys.Count);
                            if (!resolvePrivateKeysFromNatural.TryAdd(naturalPublicCursor.SortingKey, recordIndex))
                                throw new ApplicationException("!resolvePrivateKeysFromNatural.TryAdd(naturalPublicCursor.SortingKey, recordIndex)");
                            privatePrimaryKeyCanBeResolved = true;
                        }

                        if (primaryKey.CheckPublicKeyIsAssigned())
                        {
                            if (naturalPublicCursor != null) throw new ApplicationException("naturalPublicCursor != null");
                            Guid publicKey = primaryKey.PublicKey ?? throw new ApplicationException("!primaryKey.PublicKey.HasValue");
                            resolvePrivateKeysFromPublic ??= new Dictionary<Guid, int>(PrimaryKeys.Count);
                            if (!resolvePrivateKeysFromPublic.TryAdd(publicKey, recordIndex))
                                throw new ApplicationException("!resolvePrivateKeysFromPublic.TryAdd(publicKey, recordIndex)");
                            privatePrimaryKeyCanBeResolved = true;
                        }

                        if (!privatePrimaryKeyCanBeResolved)
                        {
                            if (ReadOnly) throw new ApplicationException("!privatePrimaryKeyCanBeResolved && ReadOnly");
                            if (SkipMissing) throw new ApplicationException("!privatePrimaryKeyCanBeResolved && SkipMissing");
                        }
                    }

                    if (resolvePrivateKeysFromNatural != null)
                        await ResolvePrivateKeys(resolvePrivateKeysFromNatural, cancellationToken);

                    if (resolvePrivateKeysFromPublic != null)
                        await ResolvePrivateKeys(resolvePrivateKeysFromPublic, cancellationToken);
                }
            }
        }
    }
}