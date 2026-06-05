using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourCompany.Configuration.EFCore.CollationAwareSorting;
using YourCompany.OLTP.StateOwnership.Reflection.EFCore;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public abstract partial class YourCompanyDbContextLockingRecordDataReader<TConfiguration>
    {
        public abstract partial class SingleEntityQuery<TRecordData, TQueryableRecordData>
        {
            public abstract partial class ByIds
            {
                protected virtual async Task ResolvePrivateKeys(
                    Dictionary<SortingKey, int> indeciesByNaturalKey, CancellationToken cancellationToken)
                {
                    if (indeciesByNaturalKey == null) throw new ArgumentNullException(nameof(indeciesByNaturalKey));
                    if (indeciesByNaturalKey.Count == 0) throw new ApplicationException("indeciesByNaturalKey.Count == 0");
                    EnsureReadingForReadOnlyOrByModifyingWrapperOnly();
                    if (_primaryKeysWithRecordDataByPrivateKey != null) throw new ApplicationException("_primaryKeysWithRecordDataByPrivateKey != null");

                    SortingKeyTopology.ILastProperty singleTopology = null;
                    OrElseEqualityKeys singleTopologyKeys = null;
                    Dictionary<SortingKeyTopology.ILastProperty, OrElseEqualityKeys> keysByTopology = null;
                    int iteratedKeysCount = 0;
                    foreach (var kvp in indeciesByNaturalKey)
                    {
                        var key = kvp.Key;
                        int primaryKeyIndex = kvp.Value;

                        if (keysByTopology == null)
                        {
                            singleTopology ??= key;
                            singleTopologyKeys ??= new OrElseEqualityKeys(indeciesByNaturalKey.Count);
                            if (SortingKeyTopology.Comparer.Instance.Equals(key, singleTopology))
                            {
                                singleTopologyKeys.AddAndTryRetainSingleTopology(key);
                            }
                            else
                            {
                                keysByTopology = new Dictionary<SortingKeyTopology.ILastProperty, OrElseEqualityKeys>();
                                keysByTopology.Add(singleTopology, singleTopologyKeys);
                                singleTopology = null;
                                singleTopologyKeys = null;
                                var anotherTopologyKeys = new OrElseEqualityKeys(indeciesByNaturalKey.Count - iteratedKeysCount);
                                anotherTopologyKeys.AddAndTryRetainSingleTopology(key);
                                keysByTopology.Add(key, anotherTopologyKeys);
                            }
                        }
                        else
                        {
                            if (!keysByTopology.TryGetValue(key, out var keys))
                            {
                                keys = new OrElseEqualityKeys(indeciesByNaturalKey.Count - iteratedKeysCount);
                                keysByTopology.Add(key, keys);
                            }

                            keys.AddAndTryRetainSingleTopology(key);
                        }

                        iteratedKeysCount++;
                    }

                    if (keysByTopology == null)
                    {
                        await ResolvePrivateKeys(indeciesByNaturalKey, singleTopologyKeys, cancellationToken);
                    }
                    else
                    {
                        foreach (var kvp in keysByTopology)
                            await ResolvePrivateKeys(indeciesByNaturalKey, keys: kvp.Value, cancellationToken);
                    }

                    if (indeciesByNaturalKey.Count > 0)
                    {
                        if (ReadOnly && !SkipMissing) throw new ApplicationException("indeciesByNaturalKey.Count > 0 && ReadOnly && !SkipMissing");
                        if (SkipMissing)
                            foreach (var kvp in indeciesByNaturalKey)
                                SkipMissingRecord(kvp.Value);
                        indeciesByNaturalKey.Clear();
                    }
                }

                protected virtual async Task ResolvePrivateKeys(
                    Dictionary<SortingKey, int> indeciesByNaturalKey,
                    OrElseEqualityKeys keys,
                    CancellationToken cancellationToken)
                {
                    if (indeciesByNaturalKey == null) throw new ArgumentNullException(nameof(indeciesByNaturalKey));
                    if (indeciesByNaturalKey.Count == 0) throw new ApplicationException("indeciesByNaturalKey.Count == 0");
                    if (keys == null) throw new ArgumentNullException(nameof(keys));
                    EnsureReadingForReadOnlyOrByModifyingWrapperOnly();
                    if (_primaryKeysWithRecordDataByPrivateKey != null) throw new ApplicationException("_primaryKeysWithRecordDataByPrivateKey != null");

                    string singleReplacingQueriedSetName = null;

                    for (int i = 0; i < keys.Count; i++)
                    {
                        var key = keys[i] ?? throw new ApplicationException("keys[i] == null");
                        singleReplacingQueriedSetName ??= key.ReplaceQueriedSetName;
                        if (singleReplacingQueriedSetName != key.ReplaceQueriedSetName) throw new ApplicationException("singleReplacingQueriedSetName != key.ReplaceQueriedSetName");
                    }

                    keys.ProtectFromChanges();
                    keys.ReplaceQueriedSetName = singleReplacingQueriedSetName;

                    var queriedNaturalPrivateKeyPairs = await _context.GetSingleTopologyKeys<TQueryableRecordData, long>(keys,
                        e => EF.Property<long>(e, MetadataHelper.RecordDataNeverGeneratedBigintPrimaryKeyIdPropertyName),
                        cancellationToken,
                        asNoTracking: true);
                    foreach (var pair in queriedNaturalPrivateKeyPairs)
                    {
                        var naturalKey = pair.SortingKey ?? throw new ApplicationException("naturalKey == null");
                        var privateKey = pair.ExtraValue;
                        if (!indeciesByNaturalKey.Remove(naturalKey, out var primaryKeyIndex)) throw new ApplicationException("!indeciesByNaturalKey.Remove(naturalKey, out var primaryKeyIndex)");
                        var resolvedPrimaryKey = PrimaryKeys[primaryKeyIndex] ?? throw new ApplicationException("PrimaryKeys[primaryKeyIndex] == null");
                        if (!resolvedPrimaryKey.CheckExpectsAssignment()) throw new ApplicationException("!resolvedPrimaryKey.CheckExpectsAssignment()");
                        resolvedPrimaryKey.SetPrivateKey(privateKey);
                        if (resolvedPrimaryKey.CheckPublicKeyIsAssigned()) throw new ApplicationException("resolvedPrimaryKey.CheckPublicKeyIsAssigned()");
                    }
                }
            }
        }
    }
}