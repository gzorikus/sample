using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    internal sealed class RecordState<TRecordData> : IState<TRecordData>, IStateModifying<TRecordData>
        where TRecordData : class
    {
        private static ISpecification<TRecordData>[] EmptySpecifications => Array.Empty<ISpecification<TRecordData>>();
        private List<ISpecification<TRecordData>> _matchingWithoutDataChanges;
        private List<ISpecification<TRecordData>> _matchingAfterDataChanging;

        internal IConfiguredRecordsBatch Batch { get; }
        internal int RecordIndex { get; }
        public Identity Identity { get; }
        IStateModifying<TRecordData> IState<TRecordData>.Modifying => SettingDataProperties != null ? this : null;
        public TRecordData SettingDataProperties { get; private set; }
        public TRecordData LockedRecordData { get; private set; }
        public TRecordData DataAfterAccess { get; private set; }

        public IReadOnlyList<ISpecification<TRecordData>> MatchingWithoutDataChanges
            => SettingDataProperties != null
                ? _matchingWithoutDataChanges ?? Batch.EachRecordSpecificationsToMatchWithoutDataChanges ?? EmptySpecifications
                : null;

        public IReadOnlyList<ISpecification<TRecordData>> MatchingAfterDataChanging
            => SettingDataProperties != null
                ? _matchingAfterDataChanging ?? Batch.EachRecordSpecificationsToMatchAfterDataChanging ?? EmptySpecifications
                : null;

        internal RecordState(IConfiguredRecordsBatch batch, int recordIndex, Identity identity)
        {
            if (recordIndex < 0) throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, message: null);
            Batch = batch ?? throw new ArgumentNullException(nameof(batch));
            RecordIndex = recordIndex;
            Identity = identity ?? throw new ArgumentNullException(nameof(batch));
        }

        internal void SetBeforeDataChanging(TRecordData settingDataProperties)
        {
            if (SettingDataProperties != null || this.CheckIsModifyingTransaction()) throw new ApplicationException("SettingDataProperties != null || this.CheckIsModifyingTransaction()");
            if (LockedRecordData != null || this.CheckRecordDataIsLocked()) throw new ApplicationException("LockedRecordData != null || this.CheckRecordDataIsLocked()");
            if (DataAfterAccess != null || this.CheckDataAccessIsFinished()) throw new ApplicationException("DataAfterAccess != null || this.CheckDataAccessIsFinished()");
            SettingDataProperties = settingDataProperties ?? throw new ArgumentNullException(nameof(settingDataProperties));
            this.EnsureIsBeforeDataChanging();
        }

        public void MatchBeforeDataChanging(ISpecification<TRecordData> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            this.EnsureIsBeforeDataChanging();

            var eachRecordSpecifications = Batch.EachRecordSpecificationsToMatchWithoutDataChanges;
            int eachRecordSpecificationsCount = eachRecordSpecifications?.Count ?? 0;
            if (_matchingWithoutDataChanges == null)
            {
                _matchingWithoutDataChanges = new List<ISpecification<TRecordData>>(eachRecordSpecificationsCount + 1);
                if (eachRecordSpecificationsCount > 0) _matchingWithoutDataChanges.AddRange(eachRecordSpecifications);
            }

            var unwrapped = specification.Unwrap(Identity);
            for (int i = 0; i < _matchingWithoutDataChanges.Count; i++)
                if (unwrapped.Equals(_matchingWithoutDataChanges[i].Unwrap()))
                    throw new ApplicationException("unwrapped.Equals(_matchingWithoutDataChanges[i].Unwrap())");

            _matchingWithoutDataChanges.Add(specification);
            Batch.MatchBeforeDataChanging(RecordIndex, specification);
        }

        public void ChangeDataToMatch(ISpecification<TRecordData> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            this.EnsureIsBeforeDataChanging();

            var eachRecordSpecifications = Batch.EachRecordSpecificationsToMatchAfterDataChanging;
            int eachRecordSpecificationsCount = eachRecordSpecifications?.Count ?? 0;
            if (_matchingAfterDataChanging == null)
            {
                _matchingAfterDataChanging = new List<ISpecification<TRecordData>>(eachRecordSpecificationsCount + 1);
                if (eachRecordSpecificationsCount > 0) _matchingAfterDataChanging.AddRange(eachRecordSpecifications);
            }

            var unwrapped = specification.Unwrap(Identity);
            for (int i = 0; i < _matchingAfterDataChanging.Count; i++)
                if (unwrapped.Equals(_matchingAfterDataChanging[i].Unwrap()))
                    throw new ApplicationException("unwrapped.Equals(_matchingAfterDataChanging[i].Unwrap())");

            _matchingAfterDataChanging.Add(specification);
            Batch.ChangeDataToMatch(RecordIndex, specification);
        }

        internal void SetRecordDataIsLocked(TRecordData lockedRecordData)
        {
            if (SettingDataProperties == null || !this.CheckIsModifyingTransaction()) throw new ApplicationException("SettingDataProperties == null || !this.CheckIsModifyingTransaction()");
            if (LockedRecordData != null || this.CheckRecordDataIsLocked()) throw new ApplicationException("LockedRecordData != null || this.CheckRecordDataIsLocked()");
            if (DataAfterAccess != null || this.CheckDataAccessIsFinished()) throw new ApplicationException("DataAfterAccess != null || this.CheckDataAccessIsFinished()");
            this.EnsureIsBeforeDataChanging();
            LockedRecordData = lockedRecordData ?? throw new ArgumentNullException(nameof(lockedRecordData));
            this.EnsureRecordDataIsLocked();
        }

        internal void SetDataAccessIsFinished(TRecordData dataAfterAccess)
        {
            if (SettingDataProperties != null)
            {
                if (!this.CheckIsModifyingTransaction()) throw new ApplicationException("SettingDataProperties != null && !this.CheckIsModifyingTransaction()");
                if (LockedRecordData == null || !this.CheckRecordDataIsLocked()) throw new ApplicationException("this.CheckIsModifyingTransaction() && !this.CheckRecordDataIsLocked()");
                this.EnsureRecordDataIsLocked();
                SettingDataProperties = null;
                LockedRecordData = null;
            }
            else
            {
                if (this.CheckIsModifyingTransaction()) throw new ApplicationException("SettingDataProperties == null && this.CheckIsModifyingTransaction()");
                if (LockedRecordData != null || this.CheckRecordDataIsLocked()) throw new ApplicationException("!this.CheckIsModifyingTransaction() && this.CheckRecordDataIsLocked()");
            }

            if (DataAfterAccess != null || this.CheckDataAccessIsFinished()) throw new ApplicationException("DataAfterAccess != null || this.CheckDataAccessIsFinished()");
            DataAfterAccess = dataAfterAccess ?? throw new ArgumentNullException(nameof(dataAfterAccess));
            this.EnsureDataAccessIsFinished();
        }

        internal interface IConfiguredRecordsBatch
        {
            IReadOnlyList<ISpecification<TRecordData>> EachRecordSpecificationsToMatchWithoutDataChanges { get; }
            IReadOnlyList<ISpecification<TRecordData>> EachRecordSpecificationsToMatchAfterDataChanging { get; }
            void MatchBeforeDataChanging(int recordIndex, ISpecification<TRecordData> specification);
            void ChangeDataToMatch(int recordIndex, ISpecification<TRecordData> specification);
        }
    }
}