# The Aggregate as a State Transition Function

## Preamble: Not “Yet Another Bank Transfer”

You are looking at a funds transfer between two accounts where:

- No `SELECT` happens before `UPDATE`.
- The domain aggregate has **no** references to Dapper, PostgreSQL,  
  or any persistence framework.
- `Identity` is an abstract two‑phase class.
- Specifications inherit `EventArgs` and **themselves** trigger the  
  aggregate’s callbacks.

This is **not** a “yet another CRUD example”. It shows how to push  
ORM/infrastructure details **outside the model**, turning an OLTP  
transaction into a pure closure over state.

> **Core thesis**  
> Modern ORMs (EF Core, NHibernate) teach us to see the model as a  
> data structure  
> (`class Account { public decimal Balance { get; set; } }`). The  
> framework “materialises” it from the DB, tracks changes, persists.  
> This causes two fundamental problems:
>
> 1. **Two commands per transaction** – first `SELECT` (load the  
>    object), then `UPDATE`.
> 2. **The model knows how it is saved** – via virtual properties,  
>    proxies, constructors with ~10 parameters, change tracking.
>
> In reality an OLTP model is a **state transition function**. An  
> aggregate receives an access to its **locked state** (or **default**  
> for new records) from the infrastructure, and its only job is to  
> decide whether the transition is **allowed**. Everything else is  
> infrastructure.

Below we walk through how this idea is embodied in code and show how  
much boilerplate disappears when moving to higher entry points of the  
same repository.

## 1. Comparison with the “Traditional” Approach

### Traditional pattern (read → mutate → save)

```csharp
// Pseudo‑code of a typical service
using var db = new MyDbContext();
var from = db.Accounts.Find(fromId);
var to = db.Accounts.Find(toId);
if (from.Balance < amount) throw ...;
from.Balance -= amount;
to.Balance += amount;
db.SaveChanges();
```

**Problems:**

- Two `SELECT`s (or a single `JOIN`) before the modification.
- Row locking happens only at `SaveChanges()` (usually `UPDATE` by PK).
- Concurrent modifications are detected via `RowVersion` (optimistic  
  locking), which forces retries.
- The model is coupled to `DbContext`, which ofc can be disguised  
  behind the abstractions, that never try to question the  
  two‑round‑trip Read-then-SaveChanges Magic for some reason, and  
  continue to heavily rely on this.

### Approach in our example (declare intent → one round‑trip lock+modify → verify → commit)

```csharp
await using var transaction = new DapperAccountsBatchTransaction(_connectionString);
var fromAccount = transaction.ById(new DapperAccountId(_fromPublicId));
var toAccount = transaction.ById(new DapperAccountId(_toPublicId));
fromAccount.Withdraw(_amount, _fromExpectedBalance);
toAccount.Deposit(_amount, _toExpectedBalance);
await transaction.Run();
```

- No separate `SELECT` before `Run()`.
- All updates and inserts are performed in a single CTE query that  
  locks the rows with `FOR UPDATE`, applies the changes, and returns  
  the **old** balance (from the locked snapshot) for validation.
- For new records, the infrastructure provides a **default state**  
  (e.g., `Balance = 0`) – the aggregate receives this default as the  
  “old” state and validates uniformly.
- The aggregate does not know whether the record is new or existing.
- The batch transaction itself (`DapperAccountsBatchTransaction`) implements  
  `IStateAccess` and provides the state objects to the aggregates.

## 2. Full Breakdown of the Example Code (Layer by Layer)

### 2.1. Record data – a pure contract

```csharp
internal interface IAccountRecordData {
    decimal? Balance { get; set; }
}
```

The `decimal?` type is used deliberately: the aggregate explicitly  
demands a non‑null `Balance` (throwing if it encounters `null`),  
because it uses the `decimal?` type solely to enable trivial  
assignment via the setter – `0m` is the valid default value.

### 2.2. Identity – two‑phase, type‑agnostic

```csharp
internal sealed class DapperAccountId : Identity {
    private long? _value;
    private Guid? _publicId;
    internal long Value { get; set; }
    internal Guid PublicId { get; set; }
    internal bool HasValue => _value.HasValue;
    internal bool HasPublicId => _publicId.HasValue;
    protected override bool EqualsAfterAssignment(Identity other) => ...;
    protected override int GetHashCodeAfterAssignment() => ...;
    protected override int CompareAfterAssignment(Identity other) => ...;
}
```

`Identity` can be unassigned (new aggregate), partially assigned  
(`PublicId` known, `Value` not yet), or fully assigned. This  
eliminates hacks like negative numbers or `Guid.Empty`.

### 2.3. Specification as `EventArgs` (carrier of intent + trigger)

```csharp
internal static class AccountSpecifications {
    internal static Triggering.BalanceThreshold.LessThan LessThan(decimal threshold);
    internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualNoChange(decimal threshold);
    internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualAfterIncrement(decimal threshold);
    internal static Triggering.BalanceThreshold.GreaterOrEqual GreaterOrEqualAfterSet(decimal threshold);

    abstract class Triggering : EventArgs, ISpecification<IAccountRecordData> { ... }
    abstract class BalanceThreshold : Triggering {
        internal sealed class LessThan : BalanceThreshold { ... }
        internal sealed class GreaterOrEqual : BalanceThreshold {
            internal ChangeDataToMatchMode ChangeMode { get; }
            internal void AssertChangeMode(int usedBeforeChanges, int usedAfterChanges);
            internal enum ChangeDataToMatchMode { NoChange, Increment, Set }
        }
    }
}
```

The specification simultaneously:

- tells the infrastructure which operation to perform (`NoChange`,  
  `Increment`, `Set`);
- is passed to the aggregate’s callback as an `EventArgs`, allowing  
  the aggregate to verify invariants *before* row locking (intent)  
  and *after* locking (guaranteed fresh data).

### 2.4. Aggregate – business rules only, no infrastructure

```csharp
public sealed class Account {
    internal const decimal BalanceDefault = 0m;
    private readonly IState<IAccountRecordData> _state;
    private decimal? _expectedOldBalance;

    internal Account(IStateAccess<IAccountRecordData> stateAccess)
        => _state = stateAccess?.Claim(OnTransactionCallback) ?? throw ...;

    public void Withdraw(decimal amount, decimal expectedOldBalance)
    {
        // Validate inputs, store expected old balance, set new balance via trivial setter
        _state.GetSettingPropertiesBeforeDataChanging().Balance = newBalance;
        _expectedOldBalance = expectedOldBalance;
    }

    public void Deposit(decimal amount, decimal expectedOldBalance)
    {
        // Similar to Withdraw
    }

    private void OnTransactionCallback(object sender, EventArgs e)
    {
        if (e is TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking)
        {
            // Validate locked balance against expected old balance
            // Throw ConcurrentBalanceModification on mismatch
        }
        // Handle specification-triggered callbacks for GreaterOrEqual and LessThan
    }
}
```

During a modifying transaction the aggregate **never reads**  
`Balance` from `DataAfterAccess`. It only operates on the expected  
old balance supplied from the UI/API (when the optimistic concurrency  
is needed indeed, otherwise they could just specify an increment).

For **new aggregates** (where the `Identity` has no `Value` yet), the  
infrastructure provides a **default state** (all properties at their  
initial values, e.g., `Balance = 0`) as the `LockedRecordData`. The  
aggregate does **not** know whether the record is new or existing; it  
applies the same validation logic uniformly. The persistence layer  
is also responsible to resolve the existing when only public ids  
are supplied.

## 3. State Machine Protocol: `IState`, `IStateModifying`, and Callback Phases

The state machine is the heart of the infrastructure. It is defined by  
the `IState` interface (shared by both modifying and read‑only  
transactions) and the `IStateModifying` interface (available only for  
modifying transactions). The machine transitions through well‑defined  
phases, and specific callback triggers fire at key moments.

Below is a diagram showing the phases, the available operations, and  
the callback triggers.

```plaintext
              ┌─────────────────────────────────────────────┐
              │               Transaction start             │
              └─────────────────┬───────────────────────────┘
                                │
                                ▼
              ┌─────────────────────────────────────────────┐
              │            Idle (claimed, no access)        │
              └─────────────────┬───────────────────────────┘
                                │
                                │ IState.Modifying? (yes/no)
                                │
            ┌───────────────────┴───────────────────┐
            │                                       │
            ▼ (modifying)                          ▼ (read‑only)
┌───────────────────────────┐       ┌─────────────────────────────────┐
│ BeforeDataChanging        │       │       DataAfterAccess           │
│ (can set properties via   │       │ (final state available, no      │
│  GetSettingProperties…()) │       │  modifications possible)        │
│                           │       │                                 │
│ Callback:                 │       │ Callback:                       │
│ TriggeredBeforeDataChanging│      │ (uniform phase, see below)      │
└─────────────┬─────────────┘       └─────────────────────────────────┘
              │ after properties set
              ▼
┌───────────────────────────┐
│ DataChanging              │
│ (modifications finished,  │
│  no further property sets)│
└─────────────┬─────────────┘
              │ after all aggregates modified
              ▼
┌───────────────────────────┐
│ AfterDataChanging         │
│ (ready for locking)       │
└─────────────┬─────────────┘
              │ infrastructure locks rows
              ▼
┌───────────────────────────┐
│ RecordDataLocked          │
│ (old state available via  │
│  GetLockedRecordData())   │
│                           │
│ Callback:                 │
│ TriggeredAfterRecordDataLocking│
└─────────────┬─────────────┘
              │ infrastructure applies changes to record data instance
              ▼
┌──────────────────────────────────────────┐
│          DataAfterAccess                 │
│ (final, applied state available via      │
│  IState.DataAfterAccess)                 │
│                                          │
│ Callback (uniform for both modify & RO):│
│ TriggeredAfterDataAccess                 │
│ (allows checking final invariants, e.g., │
│  "Balance is never null")                │
└─────────────┬────────────────────────────┘
              │ transaction commits/rolls back
              ▼
┌──────────────────────────────────────────┐
│ Finished                                 │
└──────────────────────────────────────────┘
```

**Key points on the phases and callbacks:**

- **`IState`** is covered with extensions fully controlling all the  
phases correlating with `TransactionCallback.ForStateAssertion`  
callback types. They throw if the state is not in the expected phase.  
The extensions which do **not** rely on `CheckIsModifyingTransaction`  
(`_state.Modifying == null` effectively) are intended for working with  
`DataAfterAccess`. These are valid in both modifying (changes applied  
to the locked record data and pending) and read‑only contexts.
- **`IStateModifying`** “extends” `IState` and adds methods for  
changing data:  
`ChangeDataToMatch(ISpecification<TRecordData> specification)` or  
`SettingDataProperties` property. This interface is **only available**  
when the transaction is modifying.
- **Phase `BeforeDataChanging`**: The aggregate sets new property  
values via `GetSettingPropertiesBeforeDataChanging()` or invokes  
`ChangeDataToMatch` with a modifying specification. The callback  
`TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging`  
fires here, allowing the aggregate to assert the trivial changes  
first (values from `SettingDataProperties`) and then the  
specifications (implementing `EventArgs`, or manually iterating  
over `_state.ForModifying().MatchingAfterDataChanging` without).
- **Phase `RecordDataLocked`**: The row has been locked  
(`ORDER BY id ... FOR UPDATE`). The old snapshot is available via  
`GetLockedRecordData()`. The callback  
`TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking`  
fires, letting the aggregate validate the old state against the  
expected value.
- **Phase `DataAfterAccess`**: The infrastructure has applied all  
changes to the in‑memory record data instance (or, for read‑only,  
has simply loaded the data). The final state with all changes applied  
is available via `_state.GetFinishedAccessData()`. The callback  
`TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess` fires  
here. This phase is essential for enforcing **final invariants** based  
on the *new* state – e.g., ensuring that `Balance` is never `null`  
(i.e., it has a meaningful **default value**). It fires uniformly for  
both transaction types (in case aggregate always `Claim` ignoring the  
lightweight `ReadOnly` access), guaranteeing consistent validation  
regardless of whether the transaction mutates data or only reads it.

This strict sequencing eliminates races and ensures that the  
aggregate’s invariants are checked both against the **old** state  
(after locking) and the **new** state (before commit).

The provided documentation block is inaccurate because the actual `BuildFullCte` implementation **does not** use:

- A direct `@ids` array (`WHERE id = ANY(@ids)`).
- A `change_mode TEXT` column.
- Direct insertion into `account` using a `public_id` as `id`.
- A simple `UNION ALL` of two update/insert CTEs.

Instead, it builds a **single composable CTE** that accepts either a static JSONB array (from `BuildJsonbChangesArraySourceSql`) or a dynamic filtered query (from `BuildJsonbQuerySourceSql`), resolves public IDs via a separate junction table, and returns a unified projection.

## 4. Persistence Layer – Composable CTE with `FOR UPDATE` and `RETURNING`

The infrastructure composes a single parameterized CTE that performs  
the entire operation in one round‑trip. Its source (`jsonb_source`)  
is **either**:

- A static `@Jsonb` parameter (direct updates), or
- A dynamic `jsonb_agg` query (filtered batch updates).

The CTE proceeds in five logical stages:

```sql
-- Stage 1: Source & expansion
WITH jsonb_source AS (                      -- either SELECT @Jsonb AS jsonb or a dynamic SELECT jsonb_agg(...)
    {jsonbSourceSql}
),
records_with_changes AS (                   -- unpack the JSONB into relational rows
    SELECT
        COALESCE(j.resolving_from_id, pid.account_id) AS resolving_from_id,
        j.balance_set,
        j.balance_increment,
        j.resolving_from_or_inserting_public_id AS resolved_from_or_inserting_public_id
    FROM jsonb_source
    CROSS JOIN LATERAL JSONB_TO_RECORDSET(jsonb_source.jsonb) AS j (
        resolving_from_id bigint,
        balance_set numeric,
        balance_increment numeric,
        resolving_from_or_inserting_public_id uuid
    )
    LEFT JOIN account_public_id pid ON pid.public_id = j.resolving_from_or_inserting_public_id
),
-- Stage 2: Lock target rows (old balance captured before update)
lock_records_to_update_wo_deadlocks AS (
    SELECT
        upd.*,
        locked.balance AS old_balance
    FROM records_with_changes upd
    JOIN account locked ON locked.id = upd.resolving_from_id
    ORDER BY locked.id                     -- ⚠️ mandatory to prevent deadlocks
    FOR UPDATE OF locked
),
-- Stage 3: Update existing records
updated_records AS (
    UPDATE account AS locked
    SET
        balance = CASE
            WHEN upd.balance_set IS NOT NULL THEN upd.balance_set
            WHEN upd.balance_increment IS NOT NULL THEN locked.balance + upd.balance_increment
            ELSE locked.balance
        END,
        last_modified_at = NOW()
    FROM lock_records_to_update_wo_deadlocks upd
    JOIN account_public_id pid ON upd.resolving_from_id = pid.account_id
    WHERE locked.id = upd.resolving_from_id
    RETURNING
        upd.resolving_from_id AS existing_record_id,
        upd.old_balance,
        pid.public_id AS existing_public_id
),
-- Stage 4: Insert new records (two-step: public_id → account)
inserted_public_ids AS (
    INSERT INTO account_public_id (public_id)
    SELECT ins.resolved_from_or_inserting_public_id
    FROM records_with_changes ins
    WHERE ins.resolving_from_id IS NULL
    RETURNING account_id, public_id
),
inserted_records AS (
    INSERT INTO account (id, balance, last_modified_at)
    SELECT
        ipi.account_id,
        CASE
            WHEN ins.balance_set IS NOT NULL THEN ins.balance_set
            WHEN ins.balance_increment IS NOT NULL THEN ins.balance_increment
            ELSE 0  -- default
        END,
        NOW()
    FROM records_with_changes ins
    JOIN inserted_public_ids ipi ON ipi.public_id = ins.resolved_from_or_inserting_public_id
    RETURNING id AS inserted_record_id
)
-- Stage 5: Unified result projection
SELECT
    COALESCE(upd.existing_record_id, ins.inserted_record_id) AS "ResolvedId",
    COALESCE(upd.old_balance, 0) AS "Balance",
    COALESCE(upd.existing_public_id, ipi.public_id) AS "ResolvedPublicId"
FROM records_with_changes ups
LEFT JOIN updated_records upd ON upd.existing_record_id = ups.resolving_from_id
LEFT JOIN inserted_public_ids ipi ON ipi.public_id = ups.resolved_from_or_inserting_public_id
LEFT JOIN inserted_records ins ON ins.inserted_record_id = ipi.account_id;
```

**Purpose of each stage:**

- **`jsonb_source`** – the composable entry point; injects either the  
  explicit change array or the result of a filtered query. All  
  parameters (JSONB array, predicate thresholds, `@BatchSize`) are  
  bound here.
- **`records_with_changes`** – unpacks the JSONB into rows. The  
  **routing logic** hinges entirely on `resolving_from_id`:
  - **If `resolving_from_id` IS NOT NULL** → the public ID is  
    **completely ignored** for routing; the row is treated as an  
    **existing** record and goes straight to the lock/update branch  
    (the `LEFT JOIN` only runs to retrieve the public ID for the final projection).
  - **If `resolving_from_id` IS NULL** → the CTE treats the public ID  
    as a **lookup key**:
    - If `LEFT JOIN account_public_id` finds a match →  
      `pid.account_id` populates `resolving_from_id`, dynamically  
      switching the row to the **update** branch (upsert by public ID).
    - If the `LEFT JOIN` yields no match →  
      `resolving_from_id` remains NULL, routing the row to the  
      **insert** branch.
  - This mechanism enables a single JSONB payload to express  
    *"update by numeric ID"*, *"update by public ID"*, or  
    *"insert new"* without requiring explicit `INSERT`/`UPDATE` flags.
- **`lock_records_to_update_wo_deadlocks`** – locks only the rows  
  that already exist in the `account` table.  
  **`ORDER BY locked.id` is strictly required** to prevent deadlocks  
  when multiple transactions lock rows in different orders. The  
  `old_balance` is captured here **before** any modifications.
- **`updated_records`** – applies changes to existing rows. The `CASE`  
  expression handles both **set‑to‑value** (`balance_set`) and  
  **increment‑by‑value** (`balance_increment`) modes natively,  
  eliminating the need for a separate `change_mode` flag. Returns the  
  updated row’s ID, the old balance (for concurrency validation),  
  and its public ID.
- **`inserted_public_ids`** – for new records (where  
  `resolving_from_id IS NULL`), inserts the public `UUID` into the  
  junction table first, generating a new numeric `account_id`.
- **`inserted_records`** – uses that freshly generated `account_id` to  
  insert the new row into `account`, applying the same balance logic.  
  Returns the new ID.
- **Final `SELECT`** – left‑joins the update and insert branches to  
  produce a consistent three‑column result set (`ResolvedId`,  
  `Balance` as old balance, `ResolvedPublicId`). This result is mapped  
  back to `LockedRecordData` and feeds the aggregate’s  
  concurrency‑check callbacks.

**Fallback for no records**: If `jsonb_source` yields zero rows (e.g.,  
an empty `@Jsonb` array or a filtered query that matches nothing),  
then `records_with_changes` is empty. All subsequent CTEs operate on  
empty sets, and the final `SELECT` returns an empty result set. The  
transaction `Run()` completes successfully with zero affected  
aggregates – this serves as the natural exit condition for batch loops  
(e.g., the bagels use‑case) without throwing exceptions.

## 5. JSON Mapping with `NpgsqlDynamicJsonAccountChanges`

The `NpgsqlDynamicJsonAccountChanges` struct serializes the  
per‑aggregate changes into a JSON array consumed by  
`JSONB_TO_RECORDSET`. Its contract directly matches the columns  
expected by the CTE’s `records_with_changes` expansion:

| JSON property | SQL column | Purpose |
|---------------|------------|---------|
| `ResolvingFromId` | `resolving_from_id` | Numeric surrogate key (for existing records – **takes precedence** if non‑null) |
| `BalanceSet` | `balance_set` | Explicit new balance value (if non‑null, update uses `SET balance = balance_set`) |
| `BalanceIncrement` | `balance_increment` | Amount to add to current balance (if non‑null, update uses `balance + increment`) |
| `ResolvingFromOrInsertingPublicId` | `resolving_from_or_inserting_public_id` | Public UUID – acts as a **key for lookup or insertion** (see routing logic in Stage 1) |

**Change mode is implicit**, not a separate field:
- If `BalanceSet` is non‑null → **Set** mode.
- Else if `BalanceIncrement` is non‑null → **Increment** mode.
- Else → **NoChange** (used for read‑only validation or filtering;  
  the balance remains unchanged).

The struct uses Npgsql’s `EnableDynamicJson` with a source‑generated  
serializer context  
(`GeneratedForInternalParentRequiringPartialAndJsonIncludeOnInternalMembers`)  
to include internal members, ensuring reflection‑free, performant  
serialization.

Domain specifications (`LessThan`, `GreaterOrEqual`) are translated  
into these persistence‑level fields:
- `GreaterOrEqualAfterIncrement(10)` → `BalanceIncrement = 10`.
- `GreaterOrEqualAfterSet(100)` → `BalanceSet = 100`.
- `GreaterOrEqualNoChange(0)` → both are `null` – used solely for  
  matching and validation (see  
  `IStateModifying.MatchingWithoutDataChanges`).

## 6. Use Case #1: Transfer Funds (simple orchestration)

```csharp
internal sealed class TransferFunds {
    internal async Task ExecuteAsync() {
        await using var transaction = _transactionFactory();
        var fromAccount = transaction.ById(new DapperAccountId(_fromPublicId));
        var toAccount = transaction.ById(new DapperAccountId(_toPublicId));
        fromAccount.Withdraw(_amount, _fromExpectedBalance);
        toAccount.Deposit(_amount, _toExpectedBalance);
        await transaction.Run();
    }
}
```

No repositories, no `SaveChanges`, no explicit `Update` calls – only  
business method invocations and an explicit `Run()`.

## 7. Use Case #2: Batch Processing with `SKIP LOCKED`

```csharp
internal sealed class GiveBagelsToPoorAccounts {
    internal async Task<int> ExecuteAsync(CancellationToken ct = default) {
        int totalUpdated = 0;
        while (!ct.IsCancellationRequested) {
            await using var transaction = _transactionFactory();
            transaction.ConfigureBatch(batchSize: 50, useForUpdateSkipLocked: true);
            transaction.MatchBeforeDataChanging(AccountSpecifications.LessThan(PowertyThreshold));
            transaction.ChangeDataToMatch(AccountSpecifications.GreaterOrEqualAfterIncrement(BagelsToGive));
            await transaction.Run();
            totalUpdated += transaction.Accounts.Count;
            if (transaction.Accounts.Count < transaction.BatchSize) break;
        }
        return totalUpdated;
    }
}
```

**Key differences from the transfer use case:**

| Feature | Transfer (6) | Batch Give Bagels (7) |
|---------|-------------|------------------------|
| **Entry** | Specific accounts by ID | Query specification (`LessThan`) |
| **Locking mode** | Default `FOR UPDATE` | `FOR UPDATE SKIP LOCKED` |
| **Number of rounds** | Single `Run()` | Loop until no rows returned |

This pattern is ideal for background job processors that need to  
process a large number of records without blocking each other.

> **Tip on batch‑level specifications**: You can add cross‑batch  
specifications to the transaction **before** calling `ById` for any  
aggregate. For example,  
`transaction.MatchBeforeDataChanging(AccountSpecifications.LessThan(100))`  
will apply to **all** records retrieved via subsequent `ById` calls  
(including new ones). These batch‑level specifications are seamlessly  
combined with per‑aggregate specifications added later (*omitted in  
the example*). This is useful for enforcing global constraints without  
cluttering the domain logic.

## 8. The Crucial Separation: Entity IDs vs. Entity Records

A fundamental design decision in this architecture is the  
**strict separation** between the entity’s identity and its record  
data. In the database, this is realised with two tables:

```sql
CREATE TABLE account_public_id (
    account_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID NOT NULL,
    CONSTRAINT uq_account_public_id UNIQUE (public_id));

CREATE TABLE account (
    id BIGINT NOT NULL PRIMARY KEY,
    balance NUMERIC(19,4) NOT NULL DEFAULT 0,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_account_public_id FOREIGN KEY (id) REFERENCES account_public_id(account_id));
```

- The `account_public_id` table maps the  
**non‑sequential public identifier** (UUID) to the  
**internal sequential private identifier** (BIGINT). This separation  
allows:

  - **No storage of the public ID in the main record table** – reduces  
    block bloat and keeps the primary key narrow.
  - **Uniform identity handling** – the same pattern works for all  
    entity types, **without** having to manually duplicate a  
    `public_id` table per entity. The `Identity` abstraction is  
    already designed to be generic; in later iterations, the  
    infrastructure becomes fully generic, eliminating per‑entity  
    ID‑tables entirely while retaining the two‑phase assignment.
  - **Shared identity across multiple aggregate slices** – in advanced  
    scenarios (e.g.,  
    `oltp-segregate-use-cases-with-transactional-composition`),  
    different aggregates can share the same underlying identity  
    (acting as different “subjects” or data slices of the same  
    entity). The separation ensures that each slice only stores its  
    specific record data, while identity management remains  
    unified and consistent.

The `DapperAccountId` class encapsulates both identifiers and  
provides a two‑phase assignment (public ID known, private ID assigned  
later). This design ensures that the aggregate never needs to know the  
internal private ID; it only works with the public ID. The  
infrastructure handles the mapping transparently.

## 9. Boilerplate: What Is Left Out (And How Higher Entry Points Reduce It)

### In the current minimal example (branch `oltp-basic-object-model-the-rest-is-up-to-you`)

You write **manually**:

- `DapperAccountsBatchTransaction` – implements `IStateAccess`;
- `DapperAccountState` – implements `IState` and `IStateModifying`,  
  bridging the domain aggregate with the Dapper adapter;
- `NpgsqlAccountSqlHelper` – translates specifications into SQL  
  fragments and composes the final CTE;
- and supporting files (`DapperAccountId`, `DapperAccountRecordData`,  
  `NpgsqlDynamicJsonAccountChanges`).

That amounts to **~800 lines** of infrastructure code for a single  
`Account` aggregate, while the domain itself is only **~300 lines**.

### When moving to the next entry point (`oltp-ways-to-access-size-limited-record-batch`)

Generic components appear:

- `IRecordsBatch<TRecord>` accessed via `IRepository<TRecord>`.
- `RecordsBatchTransactionSpecification` – reusable specifications  
  for batch operations.
- Fluent extensions that unify transaction parameterization.

Most of the hand‑written `DapperAccountsBatchTransaction` and  
`DapperAccountState` can be replaced by a shared implementation that  
works with **any** `TRecordData`. To support a **new aggregate**, the  
remaining code per aggregate shrinks to:

- An `IXxxRecordData` interface (domain).
- Specification classes for non‑standard checks (domain).
- Calling `Claim` in the aggregate constructor (domain).
- A lightweight mapping layer (still required) to transform the  
  `TRecord` to/from the Dapper structures.

The SQL generation (e.g., half of `NpgsqlAccountSqlHelper`) still  
needs to be tailored per entity because the CTE must reference the  
correct table and columns. However, this can be parameterized via  
configuration, reducing the per‑aggregate infrastructure code to  
**~300** instead of 800. This also lets you move use cases to the  
domain library since they no longer depend on concrete infrastructure.

### At the highest entry point (`oltp-di-efcore-handle-specifications-only-the-rest-is-covered`)

Full **generation** of specification‑to‑LINQ/SQL mapping is achieved  
via **EF Core**. The EF Core modules implement **pessimistic locking**  
using provider‑specific interceptors to issue `SELECT ... FOR UPDATE`  
immediately. This ensures safety against concurrent modifications.  
**However**, this pattern still performs a separate `SELECT` before  
the `UPDATE` – the classic two‑roundtrip pattern. The goal of these  
modules is **lowest‑resistance adoption**, not eliminating the extra  
read.

> **Trade‑off explained**  
The Dapper CTE approach eliminates the **second round‑trip** by  
combining locking and modification in a single statement (the CTE with  
`FOR UPDATE` and `RETURNING`). The EF Core entry point prioritises  
developer convenience and tooling over peak performance, making it  
suitable for rapid development. The key benefit is that your  
**domain logic remains decoupled** from the persistence strategy – you  
can start with EF Core and later switch to the Dapper CTE without  
touching business logic.

If performance demands a single roundtrip (as in the Dapper example),  
you should either stay with the Dapper‑based approach or extend the  
EF Core adapter with `ExecuteUpdate` (which is **not** currently part  
of the repository).

> **How would a hypothetical generic Dapper helper look?**  
> It could mirror the EF Core module but generate raw SQL from  
> specifications using `SqlKata` or a similar library. The  
> `JSONB_TO_RECORDSET` approach is already a generic pattern. With a  
> small reflection‑based library you could write:
>
> ```csharp
> await batch.UpdateAllAsync<AccountRecordData>(specifications, ...);
> ```
>
> This would reduce the current 800 lines to **~50 lines** of  
> project‑specific glue, leaving only the record type and its mapping  
configuration.

> **Important note on reading (`ReadOnly`)**  
The current example does **not** implement `IStateAccess.ReadOnly`. In  
a real system you would add it by issuing a plain `SELECT`. Its  
absence is intentional – this entry point focuses on  
**modifying transactions**. If you need read‑only queries, either  
extend this adapter or switch to an entry point that includes them.

## 10. The Freedom to Change Infrastructure Without Touching Business Logic

The most valuable outcome of this discipline is the **guarantee that  
you can later optimise or completely replace the persistence stack  
without changing a single line of business logic**.

You may start with the Dapper example shown here (one roundtrip, full  
control). Later you might discover that you need:

- **Even lower latency** – switch to raw ADO.NET with stored  
  procedures, still using the same `IStateAccess` contract.
- **A different LINQ provider** – swap Dapper for `linq2db` (which  
  can also generate `RETURNING` queries) while keeping the aggregate  
  unchanged.
- **A completely different storage engine** – move from PostgreSQL to  
  a distributed SQL database, or even to an in‑memory store for  
  testing – as long as you implement the same `IStateAccess` interface.

All these changes are **localised to the infrastructure assembly**.  
The `Account` aggregate, the `BalanceChangeSpecification` (and the  
use case ideally) stay **exactly the same**. No `#if` directives, no  
conditional compilation, no refactoring of business rules.

Conversely, you may start with the EF Core entry point (with change  
tracking and two roundtrips) because it gives you the fastest  
time‑to‑market. When your performance requirements become stricter,  
you can **smoothly migrate** to the Dapper‑based one‑roundtrip  
adapter – again without touching the domain model. The repository  
provides the entry points exactly for this reason: **you choose** the  
trade‑off fitting your current needs, and **you are never locked in**.

> **Choosing an entry point is a business decision, not a religion.**  
> Each branch in the repository represents a valid, production‑ready  
> way to implement OLTP systems. They differ in complexity,  
> performance characteristics, and the amount of hand‑written  
> infrastructure code. The repository does **not** claim that one is  
> "better" than another. It merely helps you make an informed choice  
> and **avoid common mistakes**.

## 11. A Necessary Caveat: Database Atomicity vs. Business Transaction Lifecycle

The CTE pattern demonstrates perfect **database‑level atomicity** – it  
updates two accounts (debit and credit) in a single round‑trip, with  
`FOR UPDATE` and a global `ORDER BY id` to eliminate deadlocks entirely.

So, **why isn't this how real‑world banking transfers are always implemented?**

The limitation is **not** technical (locking/performance). It is 
**architectural and regulatory**:

- **External integrations** – a transfer often involves fraud detection 
  systems, anti‑money laundering (AML) filters, tax withholding  
  services, or external clearing networks (SWIFT, SEPA, FedNow). These  
  systems have latencies measured in seconds or minutes and cannot be  
  held inside a database transaction.
- **State‑machine requirements** – a business transfer is a  
  **long‑running process** with multiple stages:  
  *Initiated → Authorised → Validated → Booked → Settled*. Each stage  
  may require a separate database transaction because the process must  
  persist its progress and safely recover from failures (e.g.,  
  timeouts from an external AML service).
- **Regulatory audit trails** – regulations often demand immutable,  
  timestamped logs for each stage of a financial operation. These logs  
  are written as separate records, often before the actual balance  
  mutation, to guarantee traceability even if the final mutation fails.

**What this means for your architecture:**

- The **persistence pattern shown here** (the CTE) is ideally suited  
  for the **"Booked"** or **"Executed"** stage – the precise moment  
  you atomically mutate the balance.
- The surrounding *business transaction* (the orchestration of fraud  
  checks, tax calculations, and external validations) must live  
  **outside** the database transaction, managed by a saga, a state  
  machine, or a workflow engine (e.g., Temporal, NServiceBus, or an  
  event handling `TransactionCallback` in a  
  **long‑running transaction** orchestrating aggregate).
- You do **not** hold PostgreSQL row locks while waiting for a SWIFT  
  response. Instead, you persist the intermediate states (e.g.,  
  `TransferState.Initiated`) in the database, and only when all  
  preconditions are met do you invoke the atomic CTE to finalise the  
  balance change.

> **Key takeaway**  
The CTE example provides the **perfect final step** for internal  
ledger mutations. The complexity of production banking lies not in  
the `UPDATE` statement, but in orchestrating the external validations  
that precede it. The pattern gives you a safe, fast, deadlock‑free  
persistence hammer – you still need to design the surrounding workflow  
to manage the nails that require external approvals.

You are right to question this block. While its **core metaphor**—the aggregate as a closure `(expectedOldState, command) -> specifications`—is accurate, the block is **incomplete** and **misleading** in two crucial ways:

1.  **It omits the critical validation phase**: The aggregate doesn't just blindly "speak the desired new state" and trust the persistence layer to sort it out. It actively participates in a **two-phase handshake** with the infrastructure. After the CTE returns the **locked actual state**, the aggregate's `TriggeredAfterRecordDataLocking` callback **validates** that the locked balance matches the `_expectedOldBalance`. This is the "No Read‑Before‑Write" safety net—if the lock reveals a different balance, the aggregate throws, and the entire transaction rolls back.
2.  **It oversimplifies the "new state" declaration**: The aggregate doesn't just provide a final balance; it provides *directives* (`SettingDataProperties`) and *constraints* (`ChangeDataToMatch`). The CTE translates these into `balance_set`, `balance_increment`, or a `CASE` that defaults to `NoChange`. This is more than a simple setter—it's a declarative **state transition specification** that the infrastructure executes atomically.

---

## 12. The Central Role of the Aggregate (Use Case Is Not the Star)

> Do not be misled by the presence of the `TransferFunds` class.  
**The center of the architecture is the `Account` aggregate.**  
The use case merely wires two aggregate instances together, and  
invokes `batchTransaction.Run()`. All business logic (sufficient  
funds, non‑negative balance, etc.) resides **inside the aggregate**.

The aggregate is built to  
**completely abstract away persistence mechanics**. It never holds a  
"live" database connection or a mutable row. Instead, it operates as a  
strict **state transition function** with a two‑phase interaction with  
the infrastructure:

| Phase | Actor | Action |
| :--- | :--- | :--- |
| **1. Intent & Directives** | Aggregate (`Claim` + `ChangeDataToMatch`) | Receives the command and the **expected old balance** (`_expectedOldBalance`). It declares the desired modifications via `SettingDataProperties` (raw balance) and adds specifications (e.g., `GreaterOrEqualAfterIncrement`) to `ChangeDataToMatch`. This produces a package of *(expected old state, new-state directives, constraints)*. |
| **2. Lock & Validate** | Persistence (CTE) + Aggregate callback | The CTE locks the actual row, applies the directives atomically, and returns the **locked old balance** via `RETURNING`. The infrastructure then calls the aggregate's `TriggeredAfterRecordDataLocking` with this actual value. The aggregate **validates** that `actualOldBalance == _expectedOldBalance`; if not, it throws, rolling back the entire transaction. |

In OOP terms, the aggregate *is* the closure over state transition:

`(expectedOldState, command) -> (directives, constraints, validationCallback)`

- `TransferFunds` is parameterizing the transaction.
- `Deposit/Withdraw` capture the closure variables  
  (`_expectedOldBalance`, `SettingDataProperties`).
- The CTE executes both transitions in the same `FOR UPDATE` batch,  
  preserving atomicity.
- After locking, each aggregate's callback checks its specific  
  `_expectedOldBalance` against `LockedRecordData`, to ensure  
  consistent transition.

This pattern moves the **source of truth** entirely into the aggregate  
while delegating **pessimistic concurrency control** and  
**atomic execution** to the persistence layer. The use case is merely  
a thin **orchestrator** that composes aggregate instances; it contains  
no `if` statements about business rules.

## 13. How to Use This Article When Navigating the Repository

- **Start here** to understand the philosophy behind the  
  `YourCompany.OLTP.StateOwnership` module.
- **Then explore** `oltp-ways-to-access-size-limited-record-batch`  
  branch to see how batching and pagination are added without  
  breaking the core model.
- **If you need EF Core integration**, switch to  
  `oltp-di-efcore-handle-specifications-only-the-rest-is-covered`  
  for a generic identity handling solution and other benefits.
- **Never treat one entry point as “better” than another** – each  
  makes a different trade‑off between control, boilerplate, feature  
  completeness, and performance. Choose the one that fits your team’s  
  ownership level and performance requirements.

## Takeaways

- **Model as a function**, not as a data structure.
- **No read‑before‑write** – use a single CTE with `FOR UPDATE` and  
  `RETURNING` to lock, modify, and obtain the previous state in one  
  round trip.
- **Specifications** carry both the intent and the trigger for  
  aggregate callbacks (by inheriting `EventArgs`).
- **Two‑phase `Identity`** eliminates infrastructure assumptions  
  (negative ids, `Guid.Empty`).
- **Separate identity from record data** to avoid bloating blocks  
  and unify identity handling across multiple aggregate slices.
- **State machine protocol** enforces correct sequencing:  
  changes intent → locking → validation of old state → validation  
  of final applied state (uniformly for both modifying and read‑only  
  transactions).
- **Higher entry points** drastically reduce boilerplate, but you must
  understand their performance characteristics.
- **You can change the entire persistence stack without touching business
  logic** – the aggregate is completely isolated over `IStateAccess`.
- **Real‑world bank transfers** require eventual consistency; this example
  focuses on the atomic part only.

# To be continued

You can continue chatting in your LLM of choice quickly by copying  
[Context.md](./Context.md). Or even more powerful, copy the  
[Prompt.md](./Prompt.md) (but make sure you update the links) - it  
will load all the files and be completely ready to continue building  
with you (see an example first [Response.md](./Response.md)).

Happy modelling, and may your aggregates stay clean of infrastructure
concerns.
