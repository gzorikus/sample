using System;

namespace YourCompany.OLTP.StateOwnership
{
    public interface IState<TRecordData> where TRecordData : class
    {
        Identity Identity { get; }
        IStateModifying<TRecordData> Modifying { get; }
        TRecordData DataAfterAccess { get; }
    }

    public static class StateExtensions
    {
        public static bool CheckIsTriggeringSpecificationToMatchWithoutDataChanges<TRecordData>(
            this IState<TRecordData> state, EventArgs transactionCallbackArgs)
            where TRecordData : class
        {
            var specification = transactionCallbackArgs as ISpecification<TRecordData>;
            if (specification == null) return false;
            if (state.Modifying == null) return true;

            ISpecification<TRecordData> singleSpecification = null;
            for (int i = 0; i < state.Modifying.MatchingWithoutDataChanges.Count; i++)
            {
                if (specification.Equals(state.Modifying.MatchingWithoutDataChanges[i]))
                {
                    if (singleSpecification != null) throw new ApplicationException("singleSpecification != null");
                    singleSpecification = specification;
                }
            }

            if (singleSpecification == null) throw new ApplicationException("singleSpecification == null");
            return true;
        }

        public static bool CheckIsTriggeringSpecificationForDataChanging<TRecordData>(
            this IState<TRecordData> state, EventArgs transactionCallbackArgs)
            where TRecordData : class
        {
            var specification = transactionCallbackArgs as ISpecification<TRecordData>;
            if (specification == null || state.Modifying == null) return false;

            ISpecification<TRecordData> singleSpecification = null;
            for (int i = 0; i < state.Modifying.MatchingAfterDataChanging.Count; i++)
            {
                if (specification.Equals(state.Modifying.MatchingAfterDataChanging[i]))
                {
                    if (singleSpecification != null) throw new ApplicationException("singleSpecification != null");
                    singleSpecification = specification;
                }
            }

            if (singleSpecification == null) throw new ApplicationException("singleSpecification == null");
            return true;
        }

        public static void EnsureIsModifyingTransaction<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException(nameof(EnsureIsModifyingTransaction));
        }

        public static void EnsureIsBeforeDataChanging<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException("!state.CheckIsModifyingTransaction()");
            if (state.CheckRecordDataIsLocked()) throw new ApplicationException("state.CheckRecordDataIsLocked()");
            if (!state.CheckIsBeforeDataChanging()) throw new ApplicationException(nameof(EnsureIsBeforeDataChanging));
        }

        public static void EnsureRecordDataIsLocked<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (!state.CheckIsModifyingTransaction()) throw new ApplicationException("!state.CheckIsModifyingTransaction()");
            if (state.CheckDataAccessIsFinished()) throw new ApplicationException("state.CheckDataAccessIsFinished()");
            if (!state.CheckRecordDataIsLocked()) throw new ApplicationException(nameof(EnsureRecordDataIsLocked));
        }

        public static void EnsureDataAccessIsFinished<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (state.CheckIsModifyingTransaction() && state.CheckRecordDataIsLocked())
                throw new ApplicationException("state.CheckIsModifyingTransaction() && state.CheckRecordDataIsLocked()");
            if (!state.CheckDataAccessIsFinished()) throw new ApplicationException(nameof(EnsureDataAccessIsFinished));
        }

        public static bool CheckIsModifyingTransaction<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
        {
            if (state.Modifying == null) return false;
            if (state.Modifying.MatchingWithoutDataChanges == null) throw new ApplicationException("state.Modifying.MatchingWithoutDataChanges == null");
            if (state.Modifying.MatchingAfterDataChanging == null) throw new ApplicationException("state.Modifying.MatchingAfterDataChanging == null");
            if (state.Modifying.SettingDataProperties == null) throw new ApplicationException("state.Modifying.SettingDataProperties == null");
            return true;
        }

        public static bool CheckIsBeforeDataChanging<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.CheckIsModifyingTransaction() && !state.CheckRecordDataIsLocked() && !state.CheckDataAccessIsFinished();

        public static bool CheckRecordDataIsLocked<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.CheckIsModifyingTransaction() && state.Modifying.LockedRecordData != null;

        public static bool CheckDataAccessIsFinished<TRecordData>(this IState<TRecordData> state)
            where TRecordData : class
            => state.DataAfterAccess != null;
    }
}