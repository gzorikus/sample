using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithIdentities
        {
            private Dictionary<Identity, int> _recordIndeciesByOriginalIdentityReference;

            protected bool TryAddModifiedRecordSpecifiedIdentityOriginalReference(Identity identity)
            {
                if (identity == null) throw new ArgumentNullException(nameof(identity));
                if (ReadOnly || !ByIds) throw new ApplicationException("ReadOnly || !ByIds");
                CurrentRunState.EnsureIsConfiguration();
                var recordIndecies = GetRecordIndeciesByOriginalIdentityReference();
                if (recordIndecies.ContainsKey(identity)) return false;
                if (_identitiesSorting) throw new ApplicationException("_identitiesSorting");
                recordIndecies.Add(identity, _identities.Count);
                return true;
            }

            protected void RestrictModifiedRecordOriginalIdentityReferencesToSortingIdentities(
                IReadOnlyList<Identity> orderedIdentities)
            {
                if (orderedIdentities == null) throw new ArgumentNullException(nameof(orderedIdentities));
                if (ReadOnly || !ByIds) throw new ApplicationException("ReadOnly || !ByIds");
                if (_identities.Count == 0 || _identitiesSorting) throw new ApplicationException("_identities.Count == 0 || _identitiesSorting");
                CurrentRunState.EnsureIsConfiguration();

                if (_recordIndeciesByOriginalIdentityReference == null) throw new ApplicationException("_recordIndeciesByOriginalIdentityReference == null");
                if (_recordIndeciesByOriginalIdentityReference.Count != _identities.Count)
                    throw new ApplicationException("_recordIndeciesByOriginalIdentityReference.Count != _identities.Count");

                try
                {
                    for (int recordIndex = 0; recordIndex < orderedIdentities.Count; recordIndex++)
                        _recordIndeciesByOriginalIdentityReference.Remove(orderedIdentities[recordIndex]);

                    if (_recordIndeciesByOriginalIdentityReference.Count > 0)
                        throw new ApplicationException("!_identitiesSorting && _recordIndeciesByOriginalIdentityReference.Count > 0");
                }
                finally
                {
                    _recordIndeciesByOriginalIdentityReference.Clear();
                    _identitiesSorting = true;
                }
            }

            protected void ReplaceOriginalIdentityReferenceAfterRead(
                int recordIndex, Identity originalIdentity, Identity replacingIdentity)
            {
                if (originalIdentity == null) throw new ArgumentNullException(nameof(originalIdentity));
                if (!ByIds) throw new ApplicationException("_identities.Count > 0 && !ByIds");
                if (AfterId != null) throw new ApplicationException("_identities.Count > 0 && AfterId != null");
                CurrentRunState.EnsureIsReadingStarted();

                if (replacingIdentity == null)
                {
                    if (!ByIdsSkipMissing) throw new ApplicationException("replacingIdentity == null && !ByIdsSkipMissing");
                    _identities[recordIndex] = null;
                    _recordIndeciesByOriginalIdentityReference?.Remove(originalIdentity);
                    IncrementByIdsSkippedMissingRecords();
                }
                else
                {
                    EnsureValidAssignedIdentityReplacementAfterReadByIds(recordIndex, originalIdentity, replacingIdentity);
                    _identities[recordIndex] = replacingIdentity;
                }
            }

            protected void EnsureValidAssignedIdentityReplacementAfterReadByIds(
                int recordIndex, Identity originalIdentity, Identity replacingIdentity)
            {
                if (originalIdentity == null) throw new ArgumentNullException(nameof(originalIdentity));
                if (replacingIdentity == null) throw new ArgumentNullException(nameof(replacingIdentity));
                if (!ByIds) throw new ApplicationException("_identities.Count > 0 && !ByIds");
                if (AfterId != null) throw new ApplicationException("_identities.Count > 0 && AfterId != null");
                CurrentRunState.EnsureIsReadingStarted();

                if (ReadOnly)
                {
                    if (!replacingIdentity.CompareBy.Reference.Equals(originalIdentity))
                    {
                        if (replacingIdentity.CheckExpectsAssignment())
                            throw new ApplicationException("ReadOnly && replacingIdentity.CheckExpectsAssignment()");
                        if (!ValidateAssignedIdentityReplacementAfterReadByIds(
                            recordIndex, originalIdentity, replacingIdentity))
                            throw new ApplicationException("!ValidateAssignedIdentityReplacementAfterReadByIds(originalIdentity, replacingIdentity, i)");
                    }
                }
                else
                {
                    var recordIndecies = GetRecordIndeciesByOriginalIdentityReference();
                    if (!recordIndecies.ContainsKey(originalIdentity))
                        throw new ApplicationException("!recordIndecies.ContainsKey(originalIdentity)");
                }
            }

            protected abstract bool ValidateAssignedIdentityReplacementAfterReadByIds(
                int recordIndex, Identity originalIdentity, Identity replacingIdentity);

            protected bool CheckToValidateAssignedIdentitiesReplacementAfterPersist()
            {
                CurrentRunState.EnsureIsPersistChangesFinished();

                if (_recordIndeciesByOriginalIdentityReference == null) return false;
                if (!ByIds) throw new ApplicationException("_recordIndeciesByOriginalIdentityReference != null && !ByIds");
                if (_recordIndeciesByOriginalIdentityReference.Count == 0) return false;
                if (_recordIndeciesByOriginalIdentityReference.Count != PresentRecordsCount)
                    throw new ApplicationException("_recordIndeciesByOriginalIdentityReference.Count != PresentRecordsCount");

                return !ReadOnly;
            }

            protected int GetRecordIndexByOriginalIdentityReference(Identity identity)
                => TryGetRecordIndexByOriginalIdentityReference(identity, out int recordIndex)
                    ? recordIndex
                    : throw new ApplicationException("!TryGetRecordIndexByIdentityReference(identity, out int recordIndex)");

            protected bool TryGetRecordIndexByOriginalIdentityReference(Identity identity, out int recordIndex)
            {
                if (identity == null) throw new ArgumentNullException(nameof(identity));
                bool retainedOriginalIdentityReference = GetRecordIndeciesByOriginalIdentityReference().TryGetValue(
                    identity, out recordIndex);
                if (retainedOriginalIdentityReference != this.CheckRecordIsPresent(recordIndex))
                    throw new ApplicationException("retainedOriginalIdentityReference != this.CheckRecordIsPresent(recordIndex)");
                return retainedOriginalIdentityReference;
            }

            private Dictionary<Identity, int> GetRecordIndeciesByOriginalIdentityReference()
            {
                if (!ByIds) throw new ApplicationException("!ByIds");
                CurrentRunState.EnsureIsBeforeReturning();

                _recordIndeciesByOriginalIdentityReference = _recordIndeciesByOriginalIdentityReference
                    ?? new Dictionary<Identity, int>(
                        _identities.Count, Identity.ReferenceComparer.AssignmentAgnostic.Instance);

                if (PresentRecordsCount > 0 && _recordIndeciesByOriginalIdentityReference.Count == 0)
                    for (int recordIndex = 0; recordIndex < _identities.Count; recordIndex++)
                        if (this.TryGetPresentRecordIdentity(recordIndex, out var identity))
                            _recordIndeciesByOriginalIdentityReference.Add(identity, recordIndex);

                if (_recordIndeciesByOriginalIdentityReference.Count != PresentRecordsCount)
                    throw new ApplicationException("_recordIndeciesByOriginalIdentityReference.Count != PresentRecordsCount");

                return _recordIndeciesByOriginalIdentityReference;
            }
        }
    }
}