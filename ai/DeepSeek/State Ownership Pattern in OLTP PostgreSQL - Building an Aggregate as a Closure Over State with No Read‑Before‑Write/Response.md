## Executive Summary

The `oltp-basic-object-model-the-rest-is-up-to-you` branch and its downstream example implementation represent a carefully architected approach to OLTP that fundamentally rejects several common ORM-driven patterns. The core thesis, articulated in the first commit, is that **"the heart of OLTP is transaction"** —one atomic action per object lifecycle. This branch deliberately strips away ORM abstractions and delegates transactional guarantees to a dedicated state access interface, enabling the domain model to remain completely infrastructure-agnostic.

---

## 1. Fundamentals from Commit Chronology

### The Foundational Commit (62511d94)

The branch's single commit establishes several non-negotiable architectural positions:

| Principle | Implementation |
|-----------|----------------|
| **Transaction as the unit of work** | Each transaction is modeled as one atomic action; read-only actions are explicitly an edge case with no relevancy guarantees |
| **No ORM-driven aggregates** | Modeling aggregates over ORM limitations is "Mistake № 1" |
| **Delegated state access** | Transactional guarantees are delegated to a dedicated `IStateAccess` interface, not to the model or "services" |
| **Callback-driven state transitions** | The aggregate accepts `IStateAccess` via constructor, claims `IState` via callback, and places results in `DataAfterAccess` |
| **Two-phase data access** | `LockedRecordData` is `null` during first callback, becomes available after database lock, and returns to `null` after access completes |

### The "No Read-Before-Write" Mandate

The context document crystalizes a critical constraint: `Claim` must NOT read data from the database. The only acceptable way to obtain existing data is through the `RETURNING` clause after `UPDATE`/`INSERT`, eliminating the "two roundtrips" anti-pattern.

---

## 2. The State Ownership Library: Interface Analysis

### `IStateAccess<TRecordData>` — The Entry Point

```csharp
public interface IStateAccess<TRecordData> where TRecordData : class {
    IState<TRecordData> ReadOnly { get; }
    IState<TRecordData> Claim(EventHandler transactionCallback);
}
```

This is intentionally minimal. `ReadOnly` exists but is secondary; `Claim(callback)` is the primary path for modifications. The aggregate receives this interface via constructor and immediately calls `Claim`, registering its callback.

### `IState<TRecordData>` — The State Container

```csharp
public interface IState<TRecordData> where TRecordData : class {
    Identity Identity { get; }
    IStateModifying<TRecordData> Modifying { get; }
    TRecordData DataAfterAccess { get; }
}
```

The interface provides three critical access points:
- `Identity` — the abstract identifier (two-phase capable)
- `Modifying` — `null` if no active modification or after access is finished
- `DataAfterAccess` — result after transaction completes

The extension methods on `IState` enforce the transaction lifecycle: `EnsureIsBeforeDataChanging()`, `EnsureRecordDataIsLocked()`, `EnsureDataAccessIsFinished()`. These are not mere suggestions—they are **runtime assertions** that validate the state machine.

### `IStateModifying<TRecordData>` — The Mutation Contract

```csharp
public interface IStateModifying<TRecordData> where TRecordData : class {
    IReadOnlyList<ISpecification<TRecordData>> MatchingWithoutDataChanges { get; }
    IReadOnlyList<ISpecification<TRecordData>> MatchingAfterDataChanging { get; }
    TRecordData SettingDataProperties { get; }
    TRecordData LockedRecordData { get; }
    void MatchBeforeDataChanging(ISpecification<TRecordData> specification);
    void ChangeDataToMatch(ISpecification<TRecordData> specification);
}
```

This interface distinguishes between two types of specifications:
1. **`MatchingWithoutDataChanges`** — conditions checked before data changes (optimistic concurrency, preconditions)
2. **`MatchingAfterDataChanging`** — state changes to apply (the "what" of the mutation)

The `SettingDataProperties` property represents the **trivial assignment path**—mutating the record data instance directly via setters. This eliminates the need for DTOs-per-command for the majority of cases.

### `Identity` — Two-Phase Assignment

The `Identity` abstract class implements a **two-phase assignment pattern**:
- `_assigned` flag prevents use before full assignment
- `ConfirmAssigned()` called only when both parts (e.g., `Value` and `PublicId`) are assigned
- `CheckExpectsAssignment()` / `EnsureWasAssigned()` enforce the protocol

The `DapperAccountId` implementation demonstrates this: `Value` (long PK) and `PublicId` (Guid) can be set independently, but `ConfirmAssigned()` is only called when both are present.

### `ISpecification<TRecordData>` — Intent as Data

```csharp
public interface ISpecification<TRecordData> where TRecordData : class {
    bool Match(TRecordData recordData);
}
```

Specifications are the **lingua franca** between domain and persistence. They carry both:
- A condition to evaluate (`Match`)
- An intent that the persistence layer must interpret (via the concrete type)

Critically, specifications can inherit `EventArgs` to trigger callbacks in the aggregate. The persistence layer reads specifications and applies them; unknown specification types **must** throw an exception.

### `TransactionCallback` — The Eventing Infrastructure

The `TransactionCallback` static class provides:
- **`GetExtraInterface<T>`** — retrieves extra interfaces from the sender
- **`Sender.IWithExtraInterfaces`** — enables senders to provide additional context
- **Three `ForStateAssertion` event types**:
  - `TriggeredBeforeDataChanging` — before any database interaction
  - `TriggeredAfterRecordDataLocking` — after `RETURNING` provides locked data
  - `TriggeredAfterDataAccess` — after access completes

This eventing model is how the aggregate receives **lifecycle notifications** without knowing anything about the persistence layer.

---

## 3. The Context Document: Architectural Principles (Non-Negotiable)

The context document establishes seven core principles:

### 1. No Read-Before-Write
`Claim` must NOT read data. The only acceptable way to obtain existing data is through `RETURNING` after `UPDATE`/`INSERT`.

### 2. Transaction as the Heart of OLTP
Each use case is a class implementing a transaction. The transaction contains the entire business operation. No hidden "unit of work" magic.

### 3. Aggregate Knows Nothing About Infrastructure
- No Dapper, no EF Core, no SQL
- No async in constructor (everything synchronous in the model layer)
- No knowledge of `Id` types (`long`, `Guid`)—only `Identity` abstraction

### 4. State Management via `IStateAccess`/`IState`/`IStateModifying`
The three interfaces form a complete state management system.

### 5. Specifications as Intent
Specifications represent conditions or instructions for the persistence layer. They can inherit `EventArgs` to trigger callbacks.

### 6. Two-Phase Identity
Identity can be partially assigned: `PublicId` may be known before persistence, `Value` only after `INSERT`.

### 7. Single Roundtrip for Batch Operations
Use PostgreSQL `JSONB_TO_RECORDSET` with a single CTE handling both `UPDATE` and `INSERT`.

---

## 4. The Example Implementation: Architecture in Action

### The Domain Aggregate (`Account`)

The `Account` class is **pure domain logic**—no infrastructure dependencies:

```csharp
public sealed class Account {
    private readonly IState<IAccountRecordData> _state;
    private decimal? _expectedOldBalance;

    internal Account(IStateAccess<IAccountRecordData> stateAccess) =>
        _state = stateAccess?.Claim(OnTransactionCallback) ?? throw ...;

    public void Withdraw(decimal amount, decimal expectedOldBalance) {
        // Business logic: validate, compute new balance
        _state.GetSettingPropertiesBeforeDataChanging().Balance = newBalance;
        _expectedOldBalance = expectedOldBalance;
    }

    private void OnTransactionCallback(object sender, EventArgs e) {
        // Handle lifecycle events
        if (e is TriggeredAfterRecordDataLocking) {
            var balance = _state.GetLockedRecordData().Balance;
            if (_expectedOldBalance.HasValue && balance != _expectedOldBalance.Value)
                throw new AccountException.ConcurrentBalanceModification(...);
        }
    }
}
```

Key observations:
- **No async** — the constructor is synchronous
- **No SQL, no Dapper** — only `IStateAccess` and `IState`
- **Concurrency check** happens in `TriggeredAfterRecordDataLocking`—the aggregate compares `_expectedOldBalance` against the locked database value
- **`SettingDataProperties`** is used for trivial assignments (`Balance = newBalance`)

### The Persistence Adapter (`DapperAccountState`)

`DapperAccountState` implements both `IState<IAccountRecordData>` and `IStateModifying<IAccountRecordData>`:

- **`SettingDataProperties`** — a mutable `DapperAccountRecordData` instance
- **`MatchingWithoutDataChanges`** / **`MatchingAfterDataChanging`** — lists of specifications
- **`LockedRecordData`** — populated after database `RETURNING`
- **`DataAfterAccess`** — populated after transaction completes

The `SetBatchSpecifications` method enables batch optimization—specifications are set once and reused.

### The Batch Transaction (`DapperAccountsBatchTransaction`)

This is the **persistence infrastructure** that implements `IStateAccess<IAccountRecordData>`:

- **Batch size limit**: `MaxBatchSize = 50`
- **`ById(DapperAccountId id)`** — tracks an account for the batch
- **`MatchBeforeDataChanging`** / **`ChangeDataToMatch`** — accumulate specifications
- **`Run()`** — executes a **single database roundtrip**

The batch transaction uses a PostgreSQL CTE with `jsonb_agg` to process multiple accounts in one roundtrip.

### SQL Generation (`NpgsqlAccountSqlHelper`)

The SQL helper demonstrates how specifications become SQL:

```csharp
private static void SetActionProperties(ref NpgsqlDynamicJsonAccountChanges changes, 
                                       ISpecification<IAccountRecordData> spec) {
    if (spec is AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual greaterOrEqual) {
        changes.BalanceChangeMode = MapJsonBalanceChangeMode(greaterOrEqual.ChangeMode);
        changes.BalanceChangeValue = greaterOrEqual.Threshold;
    } else {
        throw new ArgumentOutOfRangeException(nameof(spec));
    }
}
```

**Unknown specification types throw exceptions**—this is by design.

### Use Cases

**TransferFunds**:
- Two accounts in one transaction
- Debit from one, credit to another
- Each account has its own `_expectedOldBalance` for concurrency checking

**GiveBagelsToPoorAccounts**:
- Batch operation: find accounts with balance < threshold
- Increment each by a fixed amount
- Uses `FOR UPDATE SKIP LOCKED` to avoid contention
- Loops until no more accounts match

---

## 5. Critical Architectural Decisions

### Why No Read-Before-Write?

The `Claim` method must not read data because:
1. **Two roundtrips are an anti-pattern** in high-throughput OLTP
2. **Reads provide no relevancy guarantees**—data changes between SELECT and UPDATE
3. **`RETURNING` after UPDATE** provides the locked data with consistency guarantees

### Why Specifications as EventArgs?

Specifications inheriting `EventArgs` enables:
- **Callback triggering** — the aggregate receives notifications at specific lifecycle points
- **Decomposed assertion logic** — extra callbacks can handle complex validation
- **Persistence interpretation** — the adapter maps specifications to SQL

### Why Two-Phase Identity?

The `Identity` abstraction with two-phase assignment enables:
- **Pre-persistence identity** — `PublicId` (Guid) known before INSERT
- **Post-persistence identity** — `Value` (long PK) only after INSERT
- **Type safety** — domain never sees `long` or `Guid` directly

---

## 6. Recommendations for Extension

Based on this analysis, here are the key extension points:

| Extension Point | Approach |
|-----------------|----------|
| **New aggregate types** | Implement pure domain class accepting `IStateAccess<TRecordData>`; define `TRecordData` interface and specifications |
| **New persistence adapters** | Implement `IStateAccess<TRecordData>` and `IStateModifying<TRecordData>`; map specifications to target SQL dialect |
| **Read-only transactions** | Use `IStateAccess.ReadOnly` (rare, may be null if not supported) |
| **Trivial setters** | Use `SettingDataProperties` for simple value assignments |
| **Complex mutations** | Use `ChangeDataToMatch` with custom specifications |
| **Batch operations** | Extend `DapperAccountsBatchTransaction` pattern; maintain single roundtrip |

---

## Conclusion

The `oltp-basic-object-model-the-rest-is-up-to-you` branch and its example implementation represent a **deliberately minimal, principled foundation** for OLTP systems. The architecture:

1. **Eliminates ORM-induced complexity** by delegating state access to dedicated interfaces
2. **Enforces no read-before-write** through the `Claim`/`RETURNING` pattern
3. **Keeps the domain pure** — no infrastructure dependencies, no async in constructors
4. **Uses specifications as the contract** between domain and persistence
5. **Enables batch operations** with a single database roundtrip

The "rest is up to you" in the branch name is literal—this is a foundation, not a framework. It provides the **minimum viable abstraction** for OLTP transactions and leaves the implementation of specific aggregates, persistence adapters, and use cases to the developer. The example implementation demonstrates how to build on this foundation for both simple (transfer) and complex (batch) scenarios.