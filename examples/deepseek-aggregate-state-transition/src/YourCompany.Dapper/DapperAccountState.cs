using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper
{
    internal sealed class DapperAccountState : IState<IAccountRecordData>, IStateModifying<IAccountRecordData>
    {
        private EventHandler _callback;
        private DapperAccountRecordData _settingData;
        private List<ISpecification<IAccountRecordData>> _before;
        private List<ISpecification<IAccountRecordData>> _after;
        private IReadOnlyList<ISpecification<IAccountRecordData>> _beforeReadOnly;
        private IReadOnlyList<ISpecification<IAccountRecordData>> _afterReadOnly;
        private DapperAccountRecordData _lockedData;
        private DapperAccountRecordData _dataAfterAccess;

        internal DapperAccountState(DapperAccountId identity, EventHandler callback)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _settingData = new DapperAccountRecordData();
        }

        Identity IState<IAccountRecordData>.Identity => Identity;
        internal DapperAccountId Identity { get; }

        public IAccountRecordData SettingDataProperties => _settingData;
        public IAccountRecordData LockedRecordData => _lockedData;
        public IAccountRecordData DataAfterAccess => _dataAfterAccess;

        public IReadOnlyList<ISpecification<IAccountRecordData>> MatchingWithoutDataChanges =>
            _before ?? _beforeReadOnly ?? Array.Empty<ISpecification<IAccountRecordData>>();

        public IReadOnlyList<ISpecification<IAccountRecordData>> MatchingAfterDataChanging =>
            _after ?? _afterReadOnly ?? Array.Empty<ISpecification<IAccountRecordData>>();

        IStateModifying<IAccountRecordData> IState<IAccountRecordData>.Modifying => _settingData != null ? this : null;

        internal void SetBatchSpecifications(
            IReadOnlyList<ISpecification<IAccountRecordData>> before,
            IReadOnlyList<ISpecification<IAccountRecordData>> after)
        {
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (after == null) throw new ArgumentNullException(nameof(after));
            if (_before != null) throw new ApplicationException("_before != null");
            if (_after != null) throw new ApplicationException("_after != null");
            if (_beforeReadOnly != null) throw new ApplicationException("_beforeReadOnly != null");
            if (_afterReadOnly != null) throw new ApplicationException("_afterReadOnly != null");
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureIsBeforeDataChanging();
            _beforeReadOnly = before;
            _afterReadOnly = after;
        }

        internal void OnBeforeDataChanging()
        {
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureIsBeforeDataChanging();
            _callback.Invoke(this, TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging.Default);
        }

        public void MatchBeforeDataChanging(ISpecification<IAccountRecordData> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (_beforeReadOnly != null) throw new ApplicationException("_beforeReadOnly == null");
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureIsBeforeDataChanging();
            _before ??= new List<ISpecification<IAccountRecordData>>();
            _before.Add(specification);
            if (specification is EventArgs eventArgs)
                _callback.Invoke(this, eventArgs);
        }

        public void ChangeDataToMatch(ISpecification<IAccountRecordData> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (_afterReadOnly != null) throw new ApplicationException("_afterReadOnly != null");
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureIsBeforeDataChanging();
            _after ??= new List<ISpecification<IAccountRecordData>>();
            _after.Add(specification);
            if (specification is EventArgs eventArgs)
                _callback.Invoke(this, eventArgs);
        }

        internal void TriggerBatchSpecificationsCallbacks()
        {
            if (_callback == null) throw new ApplicationException("_callback == null");
            if (_beforeReadOnly == null || _afterReadOnly == null)
                throw new ApplicationException("_beforeReadOnly == null || _afterReadOnly == null");
            this.EnsureIsBeforeDataChanging();
            foreach (var spec in _beforeReadOnly)
                if (spec is EventArgs ea)
                    _callback.Invoke(this, ea);
            foreach (var spec in _afterReadOnly)
                if (spec is EventArgs ea)
                    _callback.Invoke(this, ea);
        }

        internal void SetLockedData(DapperAccountRecordData locked)
        {
            if (locked == null) throw new ArgumentNullException(nameof(locked));
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureIsBeforeDataChanging();
            foreach (var spec in MatchingWithoutDataChanges)
                if (!spec.Match(locked))
                    throw new ApplicationException("!spec.Match(locked)");
            _lockedData = locked;
        }

        internal void OnAfterRecordDataLocking()
        {
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureRecordDataIsLocked();
            var locked = _lockedData ?? throw new ApplicationException("_lockedData == null");

            _callback.Invoke(this, TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking.Default);

            foreach (var spec in MatchingAfterDataChanging)
                ChangeDataToMatch(spec, locked);

            if (_settingData.Balance.HasValue)
                locked.Balance = _settingData.Balance;

            _dataAfterAccess = locked;
            _lockedData = null;
            _settingData = null;
        }

        internal void OnAfterDataAccess()
        {
            if (_callback == null) throw new ApplicationException("_callback == null");
            this.EnsureDataAccessIsFinished();
            _callback.Invoke(this, TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess.Default);
            _callback = null;
        }

        private void ChangeDataToMatch(ISpecification<IAccountRecordData> spec, DapperAccountRecordData locked)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (locked == null) throw new ArgumentNullException(nameof(locked));
            this.EnsureRecordDataIsLocked();

            if (spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual)
            {
                if (_settingData.Balance.HasValue)
                    throw new ApplicationException("spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual && _settingData.Balance.HasValue");
                ChangeDataToMatch(greaterOrEqual, locked);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(spec), spec, message: null);
            }
        }

        private void ChangeDataToMatch(
            AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual,
            DapperAccountRecordData locked)
        {
            if (greaterOrEqual == null) throw new ArgumentNullException(nameof(greaterOrEqual));
            if (locked == null) throw new ArgumentNullException(nameof(locked));
            this.EnsureRecordDataIsLocked();

            switch (greaterOrEqual.ChangeMode)
            {
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Increment:
                    decimal oldBalance = locked.Balance ?? throw new ApplicationException("!locked.Balance.HasValue");
                    checked { locked.Balance = oldBalance + greaterOrEqual.Threshold; }
                    break;
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.Set:
                    locked.Balance = greaterOrEqual.Threshold;
                    break;
                case AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual.ChangeDataToMatchMode.NoChange:
                default:
                    throw new ArgumentOutOfRangeException(nameof(greaterOrEqual), greaterOrEqual, message: null);
            }
        }
    }
}