using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithIdentities
        {
            protected void AddReadIdentity(int recordIndex, Identity identity)
            {
                if (recordIndex != _identities.Count) throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, message: null);
                if (identity == null) throw new ArgumentNullException(nameof(identity));
                if (identity.CheckExpectsAssignment()) throw new ApplicationException("identity.CheckExpectsAssignment()");
                if (ByIds) throw new ApplicationException("ByIds");
                if (AfterId != null && AfterId.CheckExpectsAssignment()) throw new ApplicationException("AfterId != null && AfterId.CheckExpectsAssignment()");
                if (_recordIndeciesByOriginalIdentityReference != null) throw new ApplicationException("_recordIndeciesByOriginalIdentityReference != null");
                CurrentRunState.EnsureIsReadingStarted();

                if (AfterId != null)
                {
                    if (_identities.Count > 0)
                    {
                        if (!this.TryGetLastPresentRecordIndex(out int lastRecordIndex))
                            throw new ApplicationException("!this.TryGetLastPresentRecordIndex(out int lastRecordIndex)");
                        if (lastRecordIndex != _identities.Count - 1) throw new ApplicationException("lastRecordIndex != _identities.Count - 1");
                        if (identity <= _identities[lastRecordIndex]) throw new ApplicationException("identity <= _identities[lastRecordIndex]");
                    }
                    else
                    {
                        if (identity <= AfterId) throw new ApplicationException("AfterId != null && identity <= AfterId");
                    }
                }
                else
                {
                    if (_identities.Contains(identity)) throw new ApplicationException("_identities.Contains(identity)");
                }

                _identities.Add(identity);
            }

            protected void EnsureReadOnlyUniqueAssignedIdentitiesAfterReadByIds()
            {
                if (!ByIds) throw new ApplicationException("!ByIds");
                if (AfterId != null) throw new ApplicationException("AfterId != null");
                if (!ReadOnly) throw new ApplicationException("!ReadOnly");
                if (PresentRecordsCount == 0) throw new ApplicationException("PresentRecordsCount == 0");
                CurrentRunState.EnsureIsReadingStarted();
                EnsureUniqueAssignedIdentities();
            }

            protected override void EnsureUniqueAssignedSpecifiedIdentitiesAfterPersist()
            {
                if (!ByIds) throw new ApplicationException("!ByIds");
                if (AfterId != null) throw new ApplicationException("AfterId != null");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                if (PresentRecordsCount == 0) throw new ApplicationException("PresentRecordsCount == 0");
                CurrentRunState.EnsureIsPersistChangesFinished();

                for (int recordIndex = 0; recordIndex < _identities.Count; recordIndex++)
                    if (this.TryGetPresentRecordIdentity(recordIndex, out var identity) && identity.CheckExpectsAssignment())
                        throw new ApplicationException("this.TryGetPresentRecordIdentity(recordIndex, out var identity) && identity.CheckExpectsAssignment()");

                var uniqueByValue = EnsureUniqueAssignedIdentities();

                if (CheckToValidateAssignedIdentitiesReplacementAfterPersist())
                {
                    foreach (var kvp in _recordIndeciesByOriginalIdentityReference)
                    {
                        var originalIdentity = kvp.Key;
                        int recordIndex = kvp.Value;
                        if (originalIdentity.CheckExpectsAssignment()) throw new ApplicationException("originalIdentity.CheckExpectsAssignment()");
                        var replacingIdentity = _identities[recordIndex] ?? throw new ApplicationException("replacingIdentity == null");
                        if (!uniqueByValue.Remove(replacingIdentity)) throw new ApplicationException("!uniqueByValue.Remove(replacingIdentity)");
                        if (replacingIdentity.CompareBy.Reference.Equals(originalIdentity)) continue;
                        if (!ValidateAssignedIdentityReplacementAfterReadByIds(
                            recordIndex, originalIdentity, replacingIdentity))
                            throw new ApplicationException("!ValidateAssignedIdentityReplacementAfterReadByIds(recordIndex, originalIdentity, replacingIdentity)");
                    }

                    if (uniqueByValue.Count > 0) throw new ApplicationException("CheckToValidateAssignedIdentitiesReplacement() && uniqueByValue.Count > 0");
                }
            }

            protected HashSet<Identity> EnsureUniqueAssignedIdentities()
            {
                var uniqueByValue = new HashSet<Identity>(_identities);
                uniqueByValue.Remove(null);
                if (uniqueByValue.Count != PresentRecordsCount) throw new ApplicationException("uniqueByValue.Count != PresentRecordsCount");
                return uniqueByValue;
            }
        }
    }
}