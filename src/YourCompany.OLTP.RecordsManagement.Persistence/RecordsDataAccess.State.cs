using System;
using System.Collections.Generic;
using YourCompany.CompilerServices;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.Persistence
{
    public static partial class RecordsDataAccess
    {
        public readonly partial struct State
        {
            private readonly IStarting _recordsDataAccess;
            private readonly AfterSorting? _afterSorting;
            private readonly FinishingChain? _finishingChain;

            public bool IsAfterSortingBeforeFinishingChainSelected => _afterSorting.HasValue;
            public bool ForceReadByIdsForIdentityReplacementIfModifying { get; }
            public FinishingChain? SelectedFinishingChain => _finishingChain;

            internal State(IStarting recordsDataAccess) : this()
                => _recordsDataAccess = recordsDataAccess ?? throw new ArgumentNullException(nameof(recordsDataAccess));

            private State(State original, AfterSorting afterSorting)
            {
                if (original._afterSorting.HasValue) throw new ApplicationException("original._afterSorting.HasValue");
                if (original._finishingChain.HasValue) throw new ApplicationException("original._finishingChain.HasValue");
                if (StructHelper.IsZeroed(afterSorting)) throw new ApplicationException("StructHelper.IsZeroed(afterSorting)");
                _recordsDataAccess = original._recordsDataAccess ?? throw new ApplicationException("original._recordsDataAccess == null");
                _afterSorting = afterSorting;
                _finishingChain = null;
                ForceReadByIdsForIdentityReplacementIfModifying = afterSorting.ForceReadByIdsForIdentityReplacementIfModifying;
            }

            private State(State sortingState, FinishingChain finishingChain)
            {
                if (!sortingState._afterSorting.HasValue) throw new ApplicationException("!sortingState._afterSorting.HasValue");
                if (sortingState._finishingChain.HasValue) throw new ApplicationException("sortingState._finishingChain.HasValue");
                _recordsDataAccess = sortingState._recordsDataAccess ?? throw new ApplicationException("sortingState._recordsDataAccess == null");
                _afterSorting = null;
                _finishingChain = finishingChain.WhenBeforeFinishing();
                ForceReadByIdsForIdentityReplacementIfModifying = sortingState.ForceReadByIdsForIdentityReplacementIfModifying;
            }

            private State(State readingState, int readRecordsCount)
            {
                if (readingState._afterSorting.HasValue) throw new ApplicationException("readingState._afterSorting.HasValue");
                _recordsDataAccess = readingState._recordsDataAccess ?? throw new ApplicationException("readingState._recordsDataAccess == null");
                _afterSorting = null;
                _finishingChain = readingState.GetFinishingChain().FinishReading(readRecordsCount);
                ForceReadByIdsForIdentityReplacementIfModifying = readingState.ForceReadByIdsForIdentityReplacementIfModifying;
            }

            private State(State modifyingState, IFinish finish)
            {
                if (finish == null) throw new ArgumentNullException(nameof(finish));
                _recordsDataAccess = modifyingState._recordsDataAccess ?? throw new ApplicationException("modifyingState._recordsDataAccess == null");
                _afterSorting = null;
                _finishingChain = modifyingState.GetFinishingChain().FinishModifying(finish);
                ForceReadByIdsForIdentityReplacementIfModifying = modifyingState.ForceReadByIdsForIdentityReplacementIfModifying;
            }

            internal FinishingChain GetFinishingChain()
            {
                if (_afterSorting.HasValue)
                    throw new ApplicationException("_afterSorting.HasValue");
                return _finishingChain ?? throw new ApplicationException("!_finishingChain.HasValue");
            }

            public bool WasResetToDefault() => StructHelper.IsZeroed(this);

            internal State FinishReading(int readRecordsCount) => new State(this, readRecordsCount);
            internal State FinishModifying(IFinish finish) => new State(this, finish);

            internal State HandleSpecifiedIdentities(IReadOnlyList<Identity> orderedIdentities, bool skipMissing)
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
                if (orderedIdentities == null) throw new ArgumentNullException(nameof(orderedIdentities));
                if (orderedIdentities.Count < 1) throw new ApplicationException("orderedIdentities.Count < 1");
                if (_afterSorting.HasValue || _finishingChain.HasValue) throw new ApplicationException("_afterSorting.HasValue || _finishingChain.HasValue");

                orderedIdentities.EnsureSpecifying();
                var orderedPrimaryKeys = skipMissing ? null : orderedIdentities.AsPrimaryKeys();
                var afterSorting = orderedPrimaryKeys != null
                    ? new AfterSorting(_recordsDataAccess.ByIdsWithoutForcedReadIfModifying(orderedPrimaryKeys))
                    : default;

                if (StructHelper.IsZeroed(afterSorting))
                    afterSorting = new AfterSorting(_recordsDataAccess.ByIds(orderedIdentities, skipMissing));

                return StructHelper.IsZeroed(afterSorting) ? default : new State(this, afterSorting);
            }

            internal State HandleSortingAfterId(Identity lastSortedIdentity)
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
                if (lastSortedIdentity == null) throw new ArgumentNullException(nameof(lastSortedIdentity));
                if (lastSortedIdentity.CheckExpectsAssignment()) throw new ApplicationException("lastSortedIdentity.CheckExpectsAssignment()");
                if (_afterSorting.HasValue || _finishingChain.HasValue) throw new ApplicationException("_afterSorting.HasValue || _finishingChain.HasValue");
                var afterSorting = new AfterSorting(_recordsDataAccess.After(lastSortedIdentity));
                return StructHelper.IsZeroed(afterSorting) ? default : new State(this, afterSorting);
            }

            internal State HandleReadOnlyRecordsIncludingBeforeRead()
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
                if (_finishingChain.HasValue) throw new ApplicationException("_finishingChain.HasValue");
                var afterSorting = _afterSorting ?? new AfterSorting(_recordsDataAccess.NoSorting());
                if (StructHelper.IsZeroed(afterSorting)) return default;
                var sortingState = _afterSorting.HasValue ? this : new State(this, afterSorting);
                var readIdentity = afterSorting.ForReadOnlyWithRecordsData();
                return StructHelper.IsZeroed(readIdentity)
                    ? default
                    : new State(sortingState, new FinishingChain(readIdentity));
            }

            internal State HandleEachRecordSpecificationBeforeReadWithoutIds(object specification)
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
                if (specification == null) throw new ArgumentNullException(nameof(specification));

                State filteringState;
                IFilterBeforeRead filterBeforeRead;

                if (_afterSorting.HasValue)
                {
                    filteringState = this;
                    filterBeforeRead = _afterSorting.Value.FilterBeforeRead ?? throw new ApplicationException("_afterSorting.Value.FilterBeforeRead == null");
                }
                else if (_finishingChain.HasValue)
                {
                    if (!_finishingChain.Value.ReadOnlyRecordsIncluded) throw new ApplicationException("!!_finishingChain.Value.ReadOnlyRecordsIncluded");
                    filteringState = this;
                    filterBeforeRead = _finishingChain.Value.WhenReadOnly().WhenBeforeRead().FilterBeforeRead
                        ?? throw new ApplicationException("finishingChain.Value.FilterBeforeRead == null");
                }
                else
                {
                    var afterSorting = new AfterSorting(_recordsDataAccess.NoSorting());
                    if (StructHelper.IsZeroed(afterSorting))
                    {
                        filteringState = default;
                        filterBeforeRead = null;
                    }
                    else
                    {
                        filteringState = new State(this, afterSorting);
                        filterBeforeRead = afterSorting.FilterBeforeRead ?? throw new ApplicationException("afterSorting.FilterBeforeRead == null");
                    }
                }

                bool handled = filterBeforeRead?.FilterAfterSorting(specification) ?? false;
                return handled ? filteringState : default;
            }

            internal State EnsureReadOnlyChainBeforeRead()
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");

                State readingState;

                if (!_finishingChain.HasValue)
                {
                    var afterSorting = _afterSorting ?? new AfterSorting(_recordsDataAccess.NoSorting());
                    var readWithoutModifyingIdentity = afterSorting.ForReadOnlyWithoutRecordsData();
                    readingState = StructHelper.IsZeroed(readWithoutModifyingIdentity)
                        ? default
                        : new State(new State(this, afterSorting), new FinishingChain(readWithoutModifyingIdentity));
                }
                else
                {
                    _finishingChain.Value.WhenReadOnly().EnsureIsBeforeRead();
                    readingState = this;
                }

                return readingState;
            }

            internal State EnsureModifyingChainBeforeRead()
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");

                State modifyingState;

                if (!_finishingChain.HasValue)
                {
                    var afterSorting = _afterSorting ?? new AfterSorting(_recordsDataAccess.NoSorting());
                    var readBeforeModifyingIdentity = afterSorting.ForModifyingAfterReading();
                    modifyingState = StructHelper.IsZeroed(readBeforeModifyingIdentity)
                        ? default
                        : new State(new State(this, afterSorting), new FinishingChain(readBeforeModifyingIdentity));
                }
                else
                {
                    _finishingChain.Value.WhenModifying().EnsureIsBeforeRead();
                    modifyingState = this;
                }

                return modifyingState;
            }

            internal State EnsureModifyingChainBeforePersist()
            {
                if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");

                State modifyingState;

                if (!_finishingChain.HasValue)
                {
                    var afterSorting = _afterSorting ?? throw new ApplicationException("!_afterSorting.HasValue");
                    var finishingChain = new FinishingChain(afterSorting.ForModifyingOrCreatingByIdsWithoutReading());
                    modifyingState = StructHelper.IsZeroed(finishingChain)
                        ? default
                        : new State(this, finishingChain);
                }
                else
                {
                    _finishingChain.Value.WhenModifying().EnsureIsNoReadAllowing();
                    modifyingState = this;
                }

                return modifyingState;
            }
        }
    }
}