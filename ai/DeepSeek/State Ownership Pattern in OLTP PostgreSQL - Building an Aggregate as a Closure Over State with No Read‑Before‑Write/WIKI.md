# The Aggregate as a State Transition Function

## Table of Contents

- [Overview](#overview)
- [Core Abstractions (YourCompany.OLTP.StateOwnership)](#core-abstractions-yourcompanyoltpstateownership)
  - [IStateAccess&lt;TRecordData&gt;](#istateaccesstrecorddata)
  - [IState&lt;TRecordData&gt;](#istatetrecorddata)
  - [IStateModifying&lt;TRecordData&gt;](#istatemodifyingtrecorddata)
  - [Identity](#identity)
  - [TransactionCallback](#transactioncallback)
  - [ISpecification&lt;TRecordData&gt;](#ispecificationtrecorddata)
- [Domain Aggregate (YourCompany)](#domain-aggregate-yourcompany)
  - [Account.cs](#accountcs)
  - [AccountException.cs](#accountexceptioncs)
- [Persistence Contracts (YourCompany.Persistence)](#persistence-contracts-yourcompanypersistence)
  - [IAccountRecordData.cs](#iaccountrecorddatacs)
  - [AccountSpecifications.cs](#accountspecificationscs)
- [Infrastructure: Dapper Implementation (YourCompany.Dapper)](#infrastructure-dapper-implementation-yourcompanydapper)
  - [DapperAccountId.cs](#dapperaccountidcs)
  - [DapperAccountRecordData.cs](#dapperaccountrecorddatacs)
  - [DapperAccountState.cs](#dapperaccountstatecs)
  - [DapperAccountsBatchTransaction.cs](#dapperaccountsbatchtransactioncs)
  - [NpgsqlAccountSqlHelper.cs](#npgsqlaccountsqlhelpercs)
  - [NpgsqlDynamicJsonAccountChanges.cs](#npgsqldynamicjsonaccountchangescs)
- [Console Program & Use Cases (YourCompany.ConsoleProgram)](#console-program--use-cases-yourcompanyconsoleprogram)
  - [Program.cs](#programcs)
  - [TransferFunds.cs](#transferfundscs)
  - [GiveBagelsToPoorAccounts.cs](#givebagelstopooraccountscs)
- [Execution Flow: Call‑Tree Visualization](#execution-flow-calltree-visualization)
- [Commit History](#commit-history)
- [References](#references)

[↑ Back to TOC](#table-of-contents)

---

## Overview

This branch, `PR/the-aggregate-as-a-state-transition-function`, embodies a fundamental shift in how OLTP aggregates are modeled. The core thesis, as articulated in the [companion article](#references), is that an aggregate should be a **state transition function** – a closure over state that receives access to locked data from the infrastructure and decides whether a transition is allowed. All persistence details are pushed **outside** the model.

The implementation demonstrates:

- **No read-before-write**: a single database round‑trip locks rows, applies changes, and returns the old state for validation.
- **Pure domain aggregate** ([`Account`](#accountcs)) with no references to Dapper, PostgreSQL, or any ORM.
- **Two‑phase [`Identity`](#identity)** combining a `PublicId` (`Guid`) and a `Value` (long PK) for different access patterns.
- **Specifications as `EventArgs`** that carry both intent and trigger callbacks ([`AccountSpecifications`](#accountspecificationscs), [`ISpecification`](#ispecificationtrecorddata)).
- **Batch processing** with a single CTE query using `jsonb_agg` and `JSONB_TO_RECORDSET`.

The entry point is deliberately minimal, delegating transactional guarantees to a dedicated state access interface ([`IStateAccess<TRecordData>`](#istateaccesstrecorddata)). The model accepts its record data's state access via constructor and claims its [`IState<TRecordData>`](#istatetrecorddata) either read‑only or by providing a callback used later to assert the modeled data transition.

[↑ Back to TOC](#table-of-contents)

---

## Core Abstractions (YourCompany.OLTP.StateOwnership)

This project contains the foundational interfaces and types that define the state ownership pattern. All domain aggregates and persistence infrastructures build upon these contracts.

### IStateAccess&lt;TRecordData&gt;

**File:** [`src/YourCompany.OLTP.StateOwnership/IStateAccess.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/IStateAccess.cs)

This interface is the entry point for a domain aggregate to access its state. It is implemented by the infrastructure (e.g., [`DapperAccountsBatchTransaction`](#dapperaccountsbatchtransactioncs)) and passed to the aggregate's constructor.

**Members:**
- `IState<TRecordData> ReadOnly { get; }` – provides read‑only access to the state (not used in the current example).
- `IState<TRecordData> Claim(EventHandler<EventArgs> transactionCallback)` – claims the state for modification, registering a callback that will be invoked at various points during the transaction lifecycle.

The aggregate calls `Claim` and supplies its callback (e.g., [`Account.OnTransactionCallback`](#accountcs)). The returned [`IState<TRecordData>`](#istatetrecorddata) is then used to perform modifications and register specifications.

[↑ Back to TOC](#table-of-contents)

### IState&lt;TRecordData&gt;

**File:** [`src/YourCompany.OLTP.StateOwnership/IState.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/IState.cs)

This interface represents the state of an aggregate during a transaction.

**Members:**
- [`Identity`](#identity) `Identity { get; }` – the identity of the aggregate.
- [`IStateModifying<TRecordData>`](#istatemodifyingtrecorddata) `Modifying { get; }` – provides modifying capabilities (only available when the state is claimed for modification).
- `TRecordData DataAfterAccess { get; }` – the final data after the transaction completes.

The companion `StateExtensions` static class provides helper methods to work with the state:

- `CheckIsTriggeringSpecificationToMatchWithoutDataChanges` – checks if a specification should be matched before data changes.
- `EnsureHasSingleSpecificationToMatchWithoutDataChanges` – ensures exactly one specification of this type is registered.
- `GetSpecificationsToMatchWithoutDataChanges` – retrieves all specifications registered for matching before data changes.
- `CheckIsTriggeringSpecificationForDataChanging` – checks if a specification should be matched after data changes.
- `EnsureHasSingleSpecificationForDataChanging` – ensures exactly one specification of this type is registered.
- `GetSpecificationsForDataChanging` – retrieves all specifications registered for matching after data changes.
- `GetSettingPropertiesBeforeDataChanging` – retrieves the properties to be set before data changes (the new values).
- `GetSettingPropertiesWhileRecordDataIsLocked` – retrieves the properties to be set while the record data is locked (the new values).
- `GetLockedRecordData` – retrieves the locked record data (the old values) for validation.

These extensions are used by the aggregate and the infrastructure to interact with the state in a type‑safe manner.

[↑ Back to TOC](#table-of-contents)

### IStateModifying&lt;TRecordData&gt;

**File:** [`src/YourCompany.OLTP.StateOwnership/IStateModifying.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/IStateModifying.cs)

This interface provides the ability to modify the state and register specifications.

**Members:**
- `IReadOnlyList<ISpecification<TRecordData>> MatchingWithoutDataChanges { get; }` – specifications to be matched before data changes (e.g., balance threshold checks that should not modify data).
- `IReadOnlyList<ISpecification<TRecordData>> MatchingAfterDataChanging { get; }` – specifications to be matched after data changes (e.g., balance threshold checks that should modify data).
- `TRecordData SettingDataProperties { get; }` – the properties to be set (new values).
- `TRecordData LockedRecordData { get; }` – the locked record data (old values) for validation.
- `void MatchBeforeDataChanging(ISpecification<TRecordData> specification)` – registers a specification to be matched before data changes.
- `void ChangeDataToMatch(ISpecification<TRecordData> specification)` – registers a specification to be matched after data changes (and potentially modify data).

The distinction between `MatchBeforeDataChanging` and `ChangeDataToMatch` is crucial:
- `MatchBeforeDataChanging` – the specification is checked **before** any data is modified. It can be used for read‑only validation (e.g., "balance must be >= 0").
- `ChangeDataToMatch` – the specification is checked **after** data is modified, and it can also instruct the infrastructure on how to change the data to satisfy the specification (e.g., "set balance to 100" or "increment balance by 100").

[↑ Back to TOC](#table-of-contents)

### Identity

**File:** [`src/YourCompany.OLTP.StateOwnership/Identity.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/Identity.cs)

This abstract class represents the identity of an aggregate. It is a two‑phase identity:
1. **Unassigned** – the identity is created without a value (e.g., for a new record).
2. **Assigned** – the identity is assigned a value (e.g., after the record is created or loaded).

**Key members:**
- `bool CheckExpectsAssignment()` – returns `true` if the identity is unassigned.
- `void ConfirmAssigned()` – marks the identity as assigned (called by the infrastructure when the value is set).
- `protected abstract bool EqualsAfterAssignment(Identity other)` – equality comparison after assignment.
- `protected abstract int GetHashCodeAfterAssignment()` – hash code after assignment.
- `protected abstract int CompareAfterAssignment(Identity other)` – comparison after assignment.
- `ComparisonOptions CompareBy` – provides options for comparing identities by reference or by value.

The identity is designed to be **infrastructure‑agnostic**. In the example, [`DapperAccountId`](#dapperaccountidcs) inherits from `Identity` and provides both a `long` primary key (`Value`) and a `Guid` public identifier (`PublicId`).

[↑ Back to TOC](#table-of-contents)

### TransactionCallback

**File:** [`src/YourCompany.OLTP.StateOwnership/TransactionCallback.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/TransactionCallback.cs)

This static class defines the callback mechanism and the event types that are triggered during the transaction lifecycle.

**Key types:**
- `Sender.IWithExtraInterfaces` – an interface that allows the sender to provide extra interfaces (used for specifications).
- `TransactionCallback.ForStateAssertion` – an abstract class with three nested event types:
  - `TriggeredBeforeDataChanging` – triggered before any data changes are applied.
  - `TriggeredAfterRecordDataLocking` – triggered after the record data is locked (the old values are available).
  - `TriggeredAfterDataAccess` – triggered after the transaction completes.

The aggregate's callback ([`Account.OnTransactionCallback`](#accountcs)) receives these events and performs validation accordingly.

[↑ Back to TOC](#table-of-contents)

### ISpecification&lt;TRecordData&gt;

**File:** [`src/YourCompany.OLTP.StateOwnership/ISpecification.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/src/YourCompany.OLTP.StateOwnership/ISpecification.cs)

This interface defines a specification that can be matched against a record data instance.

**Member:**
- `bool Match(TRecordData recordData)` – returns `true` if the specification is satisfied by the given record data.

In the example, specifications are also `EventArgs` (see [`AccountSpecifications`](#accountspecificationscs)), allowing them to trigger the aggregate's callback.

[↑ Back to TOC](#table-of-contents)

---

## Domain Aggregate (YourCompany)

### Account.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany/Account.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany/Account.cs)

This is the pure domain aggregate – a **state transition function** that has no knowledge of persistence.

**Constructor:**
```csharp
internal Account(IStateAccess<IAccountRecordData> stateAccess)
    => _state = stateAccess?.Claim(OnTransactionCallback) ?? throw new ArgumentNullException(nameof(stateAccess));
```
The aggregate receives an [`IStateAccess<IAccountRecordData>`](#istateaccesstrecorddata) and immediately claims the state, registering its callback `OnTransactionCallback`.

**Properties:**
- `decimal Balance` – returns the balance after the transaction completes (using `GetFinishedAccessData`).

**Methods:**
- `void Withdraw(decimal amount, decimal expectedOldBalance)` – performs a withdrawal:
  1. Validates `amount > 0` and `expectedOldBalance >= 0`.
  2. Ensures `_expectedOldBalance` is not already set (prevents multiple modifications).
  3. Computes the new balance (`expectedOldBalance - amount`) and throws if negative.
  4. Sets the new balance on `SettingDataProperties` (retrieved via `GetSettingPropertiesBeforeDataChanging`).
  5. Stores the `expectedOldBalance` for later validation.

- `void Deposit(decimal amount, decimal expectedOldBalance)` – performs a deposit:
  1. Validates `amount > 0` and `expectedOldBalance >= 0`.
  2. Ensures `_expectedOldBalance` is not already set.
  3. Computes the new balance (`expectedOldBalance + amount`).
  4. Sets the new balance on `SettingDataProperties`.
  5. Stores the `expectedOldBalance` for later validation.

**Callback:**
```csharp
private void OnTransactionCallback(object sender, EventArgs e)
```
This is the heart of the aggregate's state transition logic:

- **`TriggeredBeforeDataChanging`** – no action (the aggregate does not need to validate before data changes).
- **`AccountSpecifications.Triggering.BalanceThreshold.GreaterOrEqual`** – handles specifications that require the balance to be >= a threshold:
  - Checks how many specifications of this type are registered for "before" and "after".
  - Ensures at most one is registered total.
  - Calls `AssertChangeMode` to validate the change mode (NoChange, Increment, Set).

- **`AccountSpecifications.Triggering.BalanceThreshold.LessThan`** – handles specifications that require the balance to be < a threshold:
  - Checks how many specifications are registered for "before" and "after".
  - Ensures at most one is registered total.
  - Throws if any are registered for "after" (this specification should only be used before data changes).

- **`TriggeredAfterRecordDataLocking`** – validates the expected old balance against the locked record data:
  - Retrieves the locked balance using `GetLockedRecordData`.
  - If the locked balance does not match `_expectedOldBalance`, throws an exception (this is the concurrency check).

**Key insight:** The aggregate never reads the database directly. It receives the locked state via [`IState<TRecordData>`](#istatetrecorddata) and uses the provided extensions to access the data. The infrastructure is responsible for locking, reading, and writing.

[↑ Back to TOC](#table-of-contents)

### AccountException.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany/AccountException.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany/AccountException.cs)

This file defines domain‑specific exceptions.

**Types:**
- `AccountException` – abstract base class with a private constructor.
- `ConcurrentBalanceModification` – thrown when the expected old balance does not match the actual locked balance. This is the concurrency detection mechanism.
- `Extension` – a commented‑out placeholder for future cross‑aggregate extensions.

[↑ Back to TOC](#table-of-contents)

---

## Persistence Contracts (YourCompany.Persistence)

### IAccountRecordData.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany/Persistence/IAccountRecordData.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany/Persistence/IAccountRecordData.cs)

This interface defines the persistence contract for an account record. It is the `TRecordData` type used throughout the example.

**Member:**
- `decimal? Balance { get; set; }` – the account balance, nullable to represent uninitialized records.

The interface is `internal` to the `YourCompany` namespace, ensuring that only the domain and persistence layers can access it.

[↑ Back to TOC](#table-of-contents)

### AccountSpecifications.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany/Persistence/AccountSpecifications.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany/Persistence/AccountSpecifications.cs)

This file defines specifications for account balances.

**Factory methods:**
- `LessThan(decimal threshold)` – creates a specification that matches accounts with balance < threshold.
- `GreaterOrEqualNoChange(decimal threshold)` – creates a specification that matches accounts with balance >= threshold, without modifying data.
- `GreaterOrEqualAfterIncrement(decimal threshold)` – creates a specification that matches accounts with balance >= threshold, and instructs the infrastructure to increment the balance by the threshold.
- `GreaterOrEqualAfterSet(decimal threshold)` – creates a specification that matches accounts with balance >= threshold, and instructs the infrastructure to set the balance to the threshold.

**Abstract base class `Triggering`:**
- Inherits from `EventArgs` and implements [`ISpecification<IAccountRecordData>`](#ispecificationtrecorddata).
- `Match` method validates the record data and calls `MatchPresent`.

**Nested class `BalanceThreshold`:**
- Abstract class with a `Threshold` property.
- `MatchPresent` retrieves the balance from the record data and calls `MatchBalance`.

**Concrete specifications:**
- `LessThan` – matches if `balance < Threshold`.
- `GreaterOrEqual` – matches if `balance >= Threshold`.
  - Has a `ChangeDataToMatchMode` enum: `NoChange`, `Increment`, `Set`.
  - `AssertChangeMode` validates that the specification is used correctly (e.g., `NoChange` should be used before data changes, `Increment` and `Set` after data changes).

**Important:** Specifications are `EventArgs`, so they trigger the aggregate's callback when registered via `MatchBeforeDataChanging` or `ChangeDataToMatch`. This allows the aggregate to validate the specification's usage.

[↑ Back to TOC](#table-of-contents)

---

## Infrastructure: Dapper Implementation (YourCompany.Dapper)

### DapperAccountId.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountId.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountId.cs)

This class implements the two‑phase identity for accounts.

**Properties:**
- `long Value` – the database primary key (assigned when the record exists).
- `Guid PublicId` – the public identifier (used for lookups).
- `bool HasValue` – `true` if the `Value` is set.
- `bool HasPublicId` – `true` if the `PublicId` is set.

**Constructors:**
- `DapperAccountId()` – unassigned identity.
- `DapperAccountId(long value)` – identity with a primary key.
- `DapperAccountId(Guid publicId)` – identity with a public identifier.

The identity is considered "assigned" when **both** `Value` and `PublicId` are set. The `ConfirmAssigned` method is called when both are set.

**Equality and comparison:** Based on the `Value` after assignment.

[↑ Back to TOC](#table-of-contents)

### DapperAccountRecordData.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountRecordData.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountRecordData.cs)

This class implements [`IAccountRecordData`](#iaccountrecorddatacs) and is used by the Dapper infrastructure.

**Properties:**
- `decimal? Balance { get; set; }` – the account balance.
- `long? ResolvedId` – the resolved primary key (used during batch processing).
- `Guid? ResolvedPublicId` – the resolved public identifier (used during batch processing).

[↑ Back to TOC](#table-of-contents)

### DapperAccountState.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountState.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountState.cs)

This class implements [`IState<IAccountRecordData>`](#istatetrecorddata) and [`IStateModifying<IAccountRecordData>`](#istatemodifyingtrecorddata).

**Fields:**
- `EventHandler<EventArgs> _callback` – the aggregate's callback.
- `DapperAccountRecordData _settingData` – the new values to be set.
- `List<ISpecification<IAccountRecordData>> _before` – specifications registered before data changes.
- `List<ISpecification<IAccountRecordData>> _after` – specifications registered after data changes.
- `IReadOnlyList<ISpecification<IAccountRecordData>> _beforeReadOnly` – read‑only list of before specifications (set by batch).
- `IReadOnlyList<ISpecification<IAccountRecordData>> _afterReadOnly` – read‑only list of after specifications (set by batch).
- `DapperAccountRecordData _lockedData` – the locked record data (old values).
- `DapperAccountRecordData _dataAfterAccess` – the data after the transaction completes.

**Constructor:**
- Accepts an [`Identity`](#identity) and a callback, initializes the `_settingData`.

**Properties:**
- `Identity Identity` – the account identity.
- `IAccountRecordData SettingDataProperties` – returns `_settingData`.
- `IAccountRecordData LockedRecordData` – returns `_lockedData`.
- `IAccountRecordData DataAfterAccess` – returns `_dataAfterAccess`.
- `IReadOnlyList<ISpecification<IAccountRecordData>> MatchingWithoutDataChanges` – returns the before specifications.
- `IReadOnlyList<ISpecification<IAccountRecordData>> MatchingAfterDataChanging` – returns the after specifications.
- `IStateModifying<IAccountRecordData> IState<IAccountRecordData>.Modifying` – returns `this` if `_settingData` is not null.

**Methods:**
- `void SetBatchSpecifications(...)` – sets the before and after specifications for batch processing.
- `void OnBeforeDataChanging()` – triggers the `TriggeredBeforeDataChanging` callback.
- `void MatchBeforeDataChanging(ISpecification<IAccountRecordData> specification)` – registers a before specification and triggers the callback (if the specification is `EventArgs`).
- `void ChangeDataToMatch(ISpecification<IAccountRecordData> specification)` – registers an after specification and triggers the callback (if the specification is `EventArgs`).
- `void TriggerBatchSpecificationsCallbacks()` – triggers callbacks for all batch specifications.

**Important:** The state is mutable only through the [`IStateModifying`](#istatemodifyingtrecorddata) interface. The aggregate sets the new values on `SettingDataProperties`, and the infrastructure reads them later to generate the SQL.

[↑ Back to TOC](#table-of-contents)

### DapperAccountsBatchTransaction.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountsBatchTransaction.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/DapperAccountsBatchTransaction.cs)

This is the core infrastructure class that implements [`IStateAccess<IAccountRecordData>`](#istateaccesstrecorddata) and manages the batch transaction.

**Constants:**
- `int MaxBatchSize = 50` – maximum batch size.

**Fields:**
- `NpgsqlDataSource _dataSource` – the PostgreSQL data source.
- `List<DapperAccountState> _accounts` – the list of account states in the transaction.
- `List<ISpecification<IAccountRecordData>> _batchConditions` – batch‑level before specifications.
- `List<ISpecification<IAccountRecordData>> _batchActions` – batch‑level after specifications.
- `DapperAccountId _pendingAccountId` – the identity of the account currently being tracked.
- `List<DapperAccountState> _registeredStates` – the list of registered states (used to prevent double registration).
- `NpgsqlConnection _connection` – the database connection.
- `NpgsqlTransaction _transaction` – the database transaction.
- `bool _disposed` – disposed flag.

**Configuration:**
- `void ConfigureBatch(int batchSize, bool useForUpdateSkipLocked = false)` – configures the batch size and locking behavior.

**Batch specification registration:**
- `void MatchBeforeDataChanging(ISpecification<IAccountRecordData> specification)` – registers a batch‑level before specification.
- `void ChangeDataToMatch(ISpecification<IAccountRecordData> specification)` – registers a batch‑level after specification.

**Account tracking:**
- `Account ById(DapperAccountId id)` – creates a new [`Account`](#accountcs) aggregate for the given identity. This method:
  1. Creates a new `DapperAccountState` with the identity and the aggregate's callback.
  2. Tracks the state in `_accounts`.
  3. Returns a new `Account` instance.

- `Account TrackAccount(DapperAccountId id)` – tracks an account and returns the aggregate.

**Execution:**
- `async Task Run()` – executes the batch transaction:
  1. Establishes a database connection and transaction.
  2. Builds the SQL query using [`NpgsqlAccountSqlHelper`](#npgsqlaccountsqlhelpercs).
  3. Executes the query using Dapper's `QueryAsync`.
  4. Processes the results, resolving identities and setting `LockedRecordData` and `DataAfterAccess`.
  5. Triggers the `TriggeredAfterRecordDataLocking` callbacks.
  6. Commits the transaction.
  7. Triggers the `TriggeredAfterDataAccess` callbacks.

**`SqlMapper.IDynamicParameters` implementation:**
- `void AddParameters(IDbCommand command, SqlMapper.Identity identity)` – adds parameters to the command.
  - Calls `NpgsqlAccountSqlHelper.BuildJsonbChangesArraySourceSql` to build the JSON changes array.
  - Calls `NpgsqlAccountSqlHelper.BuildJsonbQuerySourceSql` to build the query source.

The transaction ensures that all modifications and specifications are applied in a **single database round‑trip**, with `FOR UPDATE` locking to prevent concurrent modifications.

[↑ Back to TOC](#table-of-contents)

### NpgsqlAccountSqlHelper.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/NpgsqlAccountSqlHelper.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/NpgsqlAccountSqlHelper.cs)

This static class builds the SQL queries and parameters for the batch transaction.

**Methods:**

- `string BuildJsonbChangesArraySourceSql(IReadOnlyList<DapperAccountState> states, NpgsqlParameterCollection parameters)` – builds a JSON array of changes for each state and returns `"SELECT @Jsonb AS jsonb"`:
  1. Creates an array of [`NpgsqlDynamicJsonAccountChanges`](#npgsqldynamicjsonaccountchangescs) for each state.
  2. For each state, extracts:
     - `ResolvingFromId` – the identity value (if assigned).
     - `BalanceSet` – the new balance (from `SettingDataProperties`).
     - `ResolvingFromOrInsertingPublicId` – the public identifier (if the identity is unassigned).
  3. Processes each `MatchingAfterDataChanging` specification to set `BalanceSet` or `BalanceIncrement` via `SetRecordChangeActionProperties`.
  4. Adds the JSON array as a parameter.
  5. Returns `"SELECT @Jsonb AS jsonb"`.

- `string BuildJsonbQuerySourceSql(IReadOnlyList<ISpecification<IAccountRecordData>> batchActions, IReadOnlyList<ISpecification<IAccountRecordData>> batchConditions, bool useForUpdateSkipLocked, int batchSize, NpgsqlParameterCollection parameters)` – builds the source subquery that selects columns and applies conditions:
  1. Starts with `"id AS resolving_from_id"` as the base columns.
  2. For each batch action (specification), calls `AddBatchQueryActionColumns` to append additional columns (e.g., `@BalanceIncrement AS balance_increment` or `@BalanceSet AS balance_set`).
  3. For each batch condition (specification), calls `AddBatchQueryConditionPredicates` to build `WHERE` predicates (e.g., `balance < @BalanceLessThreshold` or `balance >= @BalanceGreaterOrEqualThreshold`).
  4. Adds a `BatchSize` parameter.
  5. Returns a subquery: `SELECT {columns} FROM account {(WHERE ...)} {(FOR UPDATE SKIP LOCKED)} LIMIT @BatchSize`.

- `string BuildFullCte(string jsonbSourceSql)` – builds the complete CTE that performs the actual update:
  1. Takes the JSON source subquery as input.
  2. Builds a CTE with multiple stages:
     - `jsonb_source` – the JSON source.
     - `records_with_changes` – parses the JSON using `JSONB_TO_RECORDSET` and joins with `account_public_id`.
     - `lock_records_to_update_wo_deadlocks` – locks the records for update and retrieves the old balance.
     - `updated_records` – performs the `UPDATE` on `account`, setting balance via `CASE` (using `balance_set` or `balance_increment`).
     - `inserted_public_ids` – inserts new public IDs.
     - `inserted_records` – inserts new accounts.
  3. Returns the final `SELECT` that coalesces results from existing and inserted records.

**Private helper methods:**
- `SetRecordChangeActionProperties` – processes a specification and sets `BalanceSet` or `BalanceIncrement` on the changes object.
- `AddBatchQueryActionColumns` – adds a column for a batch action (specification).
- `CheckToIncrementChangingBalance` – determines if the change mode is `Increment`.
- `AddBatchQueryConditionPredicates` – builds a `WHERE` predicate for a batch condition.

**Key insight:** The SQL generation is split into three parts:
1. `BuildJsonbChangesArraySourceSql` – builds the JSON changes array.
2. `BuildJsonbQuerySourceSql` – builds the source subquery that selects columns and applies conditions.
3. `BuildFullCte` – builds the complete CTE that performs the actual update, insert, and locking.

The infrastructure translates the domain intent (specifications and state) into SQL without the aggregate knowing anything about it.

[↑ Back to TOC](#table-of-contents)

### NpgsqlDynamicJsonAccountChanges.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/NpgsqlDynamicJsonAccountChanges.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.Dapper/NpgsqlDynamicJsonAccountChanges.cs)

This struct represents the changes to be applied to an account in JSON format.

**Fields (with JSON property names):**
- `long? ResolvingFromId` – the primary key of the account to update.
- `decimal? BalanceSet` – the new balance (absolute value).
- `decimal? BalanceIncrement` – the amount to increment the balance by.
- `Guid? ResolvingFromOrInsertingPublicId` – the public identifier (for new records).

**JSON serialization:** Uses `System.Text.Json` with `JsonInclude` and `JsonPropertyName` attributes. The nested `GeneratedForInternalParentRequiringPartialAndJsonIncludeOnInternalMembers` class provides the JSON serializer context.

[↑ Back to TOC](#table-of-contents)

---

## Console Program & Use Cases (YourCompany.ConsoleProgram)

### Program.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/Program.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/Program.cs)

This is the entry point of the application.

**Setup:**
1. Creates a `NpgsqlDataSourceBuilder` with the connection string.
2. Enables dynamic JSON support.
3. Builds the data source.
4. Defines a `transactionFactory` that creates [`DapperAccountsBatchTransaction`](#dapperaccountsbatchtransactioncs) instances.

**Data seeding:**
- Creates three accounts (sender, beneficiary, poorman) with initial balances.
- Uses a top‑up transaction to deposit funds.

**Use cases:**
1. [`TransferFunds`](#transferfundscs) – transfers 100 from sender to beneficiary.
2. [`GiveBagelsToPoorAccounts`](#givebagelstopooraccountscs) – gives bagels to poor accounts (balance < 10).

[↑ Back to TOC](#table-of-contents)

### TransferFunds.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/UseCases/TransferFunds.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/UseCases/TransferFunds.cs)

This use case transfers funds between two accounts.

**Constructor:**
- Accepts public identifiers for the from and to accounts, the amount, expected balances, and a transaction factory.

**ExecuteAsync:**
1. Creates a transaction.
2. Retrieves the from and to accounts by public identifier.
3. Calls [`Withdraw`](#accountcs) on the from account.
4. Calls [`Deposit`](#accountcs) on the to account.
5. Executes the transaction (calls [`Run`](#dapperaccountsbatchtransactioncs)).
6. Verifies that exactly 2 accounts were modified.

[↑ Back to TOC](#table-of-contents)

### GiveBagelsToPoorAccounts.cs

**File:** [`examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/UseCases/GiveBagelsToPoorAccounts.cs`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/src/YourCompany.ConsoleProgram/UseCases/GiveBagelsToPoorAccounts.cs)

This use case gives bagels (100) to all poor accounts (balance < 10) in batches.

**Constants:**
- `PovertyThreshold = 10m`.
- `BagelsToGive = 100m`.

**ExecuteAsync:**
1. Loops until cancellation is requested or no more accounts are updated.
2. Creates a transaction with batch size 50 and `FOR UPDATE SKIP LOCKED`.
3. Registers a before specification: [`LessThan(PovertyThreshold)`](#accountspecificationscs).
4. Registers an after specification: [`GreaterOrEqualAfterIncrement(BagelsToGive)`](#accountspecificationscs).
5. Executes the transaction (calls [`Run`](#dapperaccountsbatchtransactioncs)).
6. Accumulates the count of updated accounts.
7. Breaks if fewer accounts were updated than the batch size.

**Key insight:** This use case demonstrates batch processing with `FOR UPDATE SKIP LOCKED`, allowing the transaction to skip rows that are locked by other transactions. The batch specifications are applied to all accounts in the batch, and the infrastructure generates a single CTE query that processes all of them.

[↑ Back to TOC](#table-of-contents)

---

## Execution Flow: Call‑Tree Visualization

The following call tree traces the execution of the `Main` method through the [`TransferFunds`](#transferfundscs) use case, showing how the aggregate interacts with the infrastructure, specifications, and the database.

```
Main()
├── NpgsqlDataSourceBuilder.Build()
├── transactionFactory = () => new DapperAccountsBatchTransaction(dataSource)
├── [Seed Data]
│   └── topupTransaction = transactionFactory()
│       ├── topupTransaction.ById(new DapperAccountId(sender))
│       │   └── DapperAccountsBatchTransaction.TrackAccount()
│       │       ├── new DapperAccountState(identity, callback)
│       │       │   └── _callback = Account.OnTransactionCallback
│       │       ├── _accounts.Add(state)
│       │       └── new Account(stateAccess)
│       │           └── stateAccess.Claim(OnTransactionCallback)
│       │               └── returns IState<IAccountRecordData>
│       ├── account.Deposit(amount, expectedOldBalance)
│       │   ├── _state.GetSettingPropertiesBeforeDataChanging()
│       │   │   └── StateExtensions.GetSettingPropertiesBeforeDataChanging()
│       │   │       └── returns _state.Modifying.SettingDataProperties
│       │   ├── _settingData.Balance = newBalance
│       │   └── _expectedOldBalance = expectedOldBalance
│       ├── [repeat for beneficiary and poorman]
│       └── await topupTransaction.Run()
│           ├── Connect()
│           ├── AddParameters() [SqlMapper.IDynamicParameters]
│           │   ├── NpgsqlAccountSqlHelper.BuildJsonbChangesArraySourceSql()
│           │   │   └── for each state:
│           │   │       ├── new NpgsqlDynamicJsonAccountChanges
│           │   │       │   ├── ResolvingFromId = state.Identity.Value (if assigned)
│           │   │       │   ├── BalanceSet = state.SettingDataProperties.Balance
│           │   │       │   └── ResolvingFromOrInsertingPublicId = state.Identity.PublicId (if unassigned)
│           │   │       └── for each spec in state.MatchingAfterDataChanging:
│           │   │           └── SetRecordChangeActionProperties()
│           │   │               └── if spec is GreaterOrEqual:
│           │   │                   ├── if ChangeMode == Increment: changes.BalanceIncrement = threshold
│           │   │                   └── else: changes.BalanceSet = threshold
│           │   └── NpgsqlAccountSqlHelper.BuildJsonbQuerySourceSql()
│           │       ├── build SELECT columns (id AS resolving_from_id)
│           │       ├── for each batchAction (spec):
│           │       │   └── AddBatchQueryActionColumns()
│           │       │       └── if spec is GreaterOrEqual with Set/Increment:
│           │       │           └── add CASE expression for balance update
│           │       ├── for each batchCondition (spec):
│           │       │   └── AddBatchQueryConditionPredicates()
│           │       │       └── if spec is LessThan: add "balance < @pN"
│           │       └── add FOR UPDATE SKIP LOCKED (if configured)
│           ├── _connection.QueryAsync(sql, param: this, transaction)
│           │   └── [PostgreSQL executes CTE with JSONB_TO_RECORDSET]
│           │       ├── locks rows with FOR UPDATE
│           │       ├── applies updates (balance = balance + increment, or balance = set)
│           │       └── returns jsonb_agg of resolved rows
│           ├── for each row in result:
│           │   ├── resolve DapperAccountId (set Value and PublicId)
│           │   ├── state._lockedData = row (old balance)
│           │   └── state._dataAfterAccess = row (new balance)
│           ├── for each state in _accounts:
│           │   └── OnAfterRecordDataLocking()
│           │       └── _callback.Invoke(this, TriggeredAfterRecordDataLocking.Default)
│           │           └── Account.OnTransactionCallback(sender, e)
│           │               └── if e is TriggeredAfterRecordDataLocking:
│           │                   ├── balance = _state.GetLockedRecordData().Balance
│           │                   ├── if balance != _expectedOldBalance:
│           │                   │   └── throw new AccountException.ConcurrentBalanceModification
│           │                   └── [validation passed]
│           ├── _transaction.Commit()
│           └── for each state in _accounts:
│               └── OnAfterDataAccess()
│                   └── _callback.Invoke(this, TriggeredAfterDataAccess.Default)
│                       └── Account.OnTransactionCallback(sender, e)
│                           └── [no action]
├── transfer = new TransferFunds(...)
│   └── ExecuteAsync()
│       ├── transaction = _transactionFactory()
│       ├── fromAccount = transaction.ById(new DapperAccountId(_fromPublicId))
│       │   └── [same as above, but with PublicId lookup]
│       ├── toAccount = transaction.ById(new DapperAccountId(_toPublicId))
│       │   └── [same as above]
│       ├── fromAccount.Withdraw(_amount, _fromExpectedBalance)
│       │   ├── _state.GetSettingPropertiesBeforeDataChanging()
│       │   │   └── returns _settingData
│       │   ├── _settingData.Balance = newBalance (expectedOldBalance - amount)
│       │   └── _expectedOldBalance = expectedOldBalance
│       ├── toAccount.Deposit(_amount, _toExpectedBalance)
│       │   ├── _state.GetSettingPropertiesBeforeDataChanging()
│       │   │   └── returns _settingData
│       │   ├── _settingData.Balance = newBalance (expectedOldBalance + amount)
│       │   └── _expectedOldBalance = expectedOldBalance
│       ├── await transaction.Run()
│       │   └── [same as above, with two accounts]
│       └── if (transaction.Accounts.Count != 2) throw
├── Console.WriteLine("Transfer succeeded")
├── giveBagels = new GiveBagelsToPoorAccounts(transactionFactory)
│   └── ExecuteAsync()
│       ├── while (!ct.IsCancellationRequested)
│       │   ├── transaction = _transactionFactory()
│       │   ├── transaction.ConfigureBatch(batchSize: 50, useForUpdateSkipLocked: true)
│       │   ├── transaction.MatchBeforeDataChanging(AccountSpecifications.LessThan(PovertyThreshold))
│       │   │   └── _batchConditions.Add(specification)
│       │   ├── transaction.ChangeDataToMatch(AccountSpecifications.GreaterOrEqualAfterIncrement(BagelsToGive))
│       │   │   └── _batchActions.Add(specification)
│       │   ├── await transaction.Run()
│       │   │   ├── [builds SQL with WHERE balance < @p AND FOR UPDATE SKIP LOCKED]
│       │   │   ├── [executes CTE with balance = balance + BagelsToGive]
│       │   │   ├── [processes results, validates via callback]
│       │   │   └── [commits]
│       │   ├── totalUpdated += transaction.Accounts.Count
│       │   └── if (transaction.Accounts.Count < transaction.BatchSize) break
│       └── return totalUpdated
└── Console.WriteLine($"Bagels given to {count} poor accounts.")
```

**Key moments in the flow:**

1. **Callback registration** – When [`Account`](#accountcs) is constructed, it calls [`IStateAccess<TRecordData>.Claim`](#istateaccesstrecorddata) with [`OnTransactionCallback`](#accountcs). This registers the aggregate's callback with the infrastructure (`DapperAccountState._callback`).

2. **Modification registration** – [`Withdraw`](#accountcs) and [`Deposit`](#accountcs) call `_state.GetSettingPropertiesBeforeDataChanging()` to obtain the `SettingDataProperties` and set the new balance. They also store `_expectedOldBalance` for later validation.

3. **Specification registration** – In the [`GiveBagelsToPoorAccounts`](#givebagelstopooraccountscs) use case, specifications are registered at the batch level via [`MatchBeforeDataChanging`](#istatemodifyingtrecorddata) and [`ChangeDataToMatch`](#istatemodifyingtrecorddata). These are stored in `_batchConditions` and `_batchActions`.

4. **SQL generation** – When [`Run`](#dapperaccountsbatchtransactioncs) is called, [`NpgsqlAccountSqlHelper`](#npgsqlaccountsqlhelpercs) builds the SQL:
   - [`BuildJsonbChangesArraySourceSql`](#npgsqlaccountsqlhelpercs) creates a JSON array of changes for each account.
   - [`BuildJsonbQuerySourceSql`](#npgsqlaccountsqlhelpercs) builds the main query with `WHERE` predicates from batch conditions and `CASE` expressions from batch actions.
   - The infrastructure translates the domain intent (specifications and state) into SQL without the aggregate knowing anything about it.

5. **Locking** – The SQL includes `FOR UPDATE SKIP LOCKED` when configured. This locks the rows at the database level, preventing concurrent modifications.

6. **Concurrency check** – After the query executes and the rows are locked, the [`TriggeredAfterRecordDataLocking`](#transactioncallback) callback is triggered. The aggregate retrieves the locked balance using `GetLockedRecordData` and validates it against `_expectedOldBalance`. If they don't match, a [`ConcurrentBalanceModification`](#accountexceptioncs) exception is thrown.

7. **Transaction commit** – After all callbacks complete successfully, the transaction is committed.

8. **Extension points** – The pattern is extensible in several ways:
   - New aggregates can implement the same pattern by accepting [`IStateAccess<TRecordData>`](#istateaccesstrecorddata) and providing a callback.
   - New specifications can be added by implementing [`ISpecification<TRecordData>`](#ispecificationtrecorddata) and inheriting from `EventArgs`.
   - New persistence infrastructures can implement [`IStateAccess<TRecordData>`](#istateaccesstrecorddata) and the related interfaces.
   - The [`Identity`](#identity) class can be extended for different identifier types.

[↑ Back to TOC](#table-of-contents)

---

## Commit History

The following commits are part of the branch `PR/the-aggregate-as-a-state-transition-function` and its preceding history:

- **`init`** – [`f4b778c`](https://github.com/gzorikus/sample/commit/f4b778c544cefeb2a61f3175d321798fe0cbe34f) – Initial repository setup.

- **`0|: welcome aboard`** – [`ce0d35c`](https://github.com/gzorikus/sample/commit/ce0d35c1ccda4058cb7842bd1675ca47fca239fd) – Initial project structure.

- **`fixup! 0|: welcome aboard`** – [`05313ca`](https://github.com/gzorikus/sample/commit/05313ca292f0cde4d3905f55818f1f586917b902) – Editorconfig + prep conflictless rebase.

- **`1|: the heart of OLTP is transaction`** – [`62511d9`](https://github.com/gzorikus/sample/commit/62511d94f4523eaa9b52d00c625848176a656da4) – Core transaction abstraction with [`IStateAccess`](#istateaccesstrecorddata), [`IState`](#istatetrecorddata), [`IStateModifying`](#istatemodifyingtrecorddata), [`Identity`](#identity), [`TransactionCallback`](#transactioncallback), and [`ISpecification`](#ispecificationtrecorddata). This commit introduces the fundamental state ownership pattern.

- **`fixup! 1|: the heart of OLTP is transaction`** – [`6a5ed2a`](https://github.com/gzorikus/sample/commit/6a5ed2a447b9321e040392d00c82dff1dc5e0ce7) – Added `StateExtensions` for expressing state expectations.

- **`fixup! 1|: the heart of OLTP is transaction`** – [`183a93b`](https://github.com/gzorikus/sample/commit/183a93b2566353153b28c62cedd7e15bfa4d170f) – Final alignment with the reference repository, including the `deepseek-aggregate-state-transition` example.

[↑ Back to TOC](#table-of-contents)

---

## References

- **Branch:** [`PR/the-aggregate-as-a-state-transition-function`](https://github.com/gzorikus/sample/tree/PR/the-aggregate-as-a-state-transition-function)
- **Base branch:** `oltp-basic-object-model-the-rest-is-up-to-you` (mentioned in the example README)
- **Companion article:** [The Aggregate as a State Transition Function](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/ai/DeepSeek/State%20Ownership%20Pattern%20in%20OLTP%20PostgreSQL%20-%20Building%20an%20Aggregate%20as%20a%20Closure%20Over%20State%20with%20No%20Read‑Before‑Write/The%20Aggregate%20as%20a%20State%20Transition%20Function.md) – Explains the rationale, trade‑offs, and comparison with traditional approaches.
- **Full repository README:** [`README.md`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/README.md) – Overview of the repository goals, metrics, and project structure.
- **Example README:** [`examples/deepseek-aggregate-state-transition/README.md`](https://github.com/gzorikus/sample/blob/PR/the-aggregate-as-a-state-transition-function/examples/deepseek-aggregate-state-transition/README.md) – Step‑by‑step guide to running the example.

[↑ Back to TOC](#table-of-contents)