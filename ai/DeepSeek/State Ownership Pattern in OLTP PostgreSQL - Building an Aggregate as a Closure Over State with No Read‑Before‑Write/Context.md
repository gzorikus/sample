# Context for Implementing Features on Top of the `oltp-basic-object-model-the-rest-is-up-to-you` Entry Point

## Original Repository

**GitHub:** https://github.com/gzorikus/sample
**Entry Point Branch:** `oltp-basic-object-model-the-rest-is-up-to-you`
**Core Module:** `YourCompany.OLTP.StateOwnership`

This context captures the architectural decisions, constraints, and patterns established during the development of a complete banking transaction example (account balance transfer) using Dapper and PostgreSQL. The goal is to enable you to build **new features** on this foundation without repeating the same design mistakes.

---

## Core Architectural Principles (Non‑Negotiable)

1. **No Read‑Before‑Write**
   `Claim` must NOT read data from the database. The only acceptable way to obtain existing data is through a single CTE that locks, modifies, and returns the **old** state (from the locked row) in one round‑trip (or a **default** for new records). For new aggregates, there is no row to lock; the CTE's `INSERT` branch provides a default initial state (e.g., `Balance = 0`) that the aggregate's callback can validate uniformly. The aggregate never knows whether the record is new or existing.

2. **Transaction as the Heart of OLTP**
   Each use case is a class implementing a transaction. The transaction contains the entire business operation. There is no hidden “unit of work” magic.

3. **Aggregate Knows Nothing About Infrastructure**
   - No Dapper, no EF Core, no SQL.
   - No async in constructor (everything synchronous in the model layer).
   - No knowledge of `Id` types (`long`, `Guid`) – only `Identity` abstraction.

4. **State Management via `IStateAccess<TRecordData>` / `IState<TRecordData>` / `IStateModifying<TRecordData>`**
   - `IStateAccess.ReadOnly` – for reads (rare, can be `null` if not supported).
   - `IStateAccess.Claim(callback)` – for modifications, passes a callback that the aggregate provides.
   - `IState.DataAfterAccess` – result after transaction completes (may be `null` before).
   - `IState.Modifying` – `null` if no active modification or after access is finished.
   - `IStateModifying.SettingDataProperties` – mutable record data for trivial assignments (setters).
   - `IStateModifying.LockedRecordData` – for existing records, the locked **old** data from the database (after the CTE returns). For new records, the infrastructure must supply an instance with all properties set to their default values **and guarantee that `Balance` is never `null`**. The aggregate explicitly demands a non‑`null` `Balance` (throwing if it encounters `null`), because it uses the `decimal?` type solely to enable trivial assignment via the setter—`0m` is the valid default value to set. If the persistence layer fails to provide a non‑null `Balance` for a new record, the model will throw a clear exception, making the requirement explicit and verifiable at runtime. The aggregate treats both uniformly.

5. **Specifications as Intent**
   - `ISpecification<T>` represents a condition or an instruction for the persistence layer.
   - Specifications can inherit `EventArgs` to trigger callbacks in the aggregate.
   - The aggregate adds specifications via `MatchBeforeDataChanging` (for optimistic concurrency checks) and `ChangeDataToMatch` (for desired state changes).
   - The persistence layer reads specifications and applies them. Unknown specification types MUST throw an exception.

6. **Two‑Phase Identity (`AccountId` example)**
   - `Identity` is abstract, requires a concrete implementation.
   - Identity can be partially assigned: `PublicId` (Guid) may be known before persistence, `Value` (long, PK) only after INSERT.
   - `ConfirmAssigned()` is called only when both parts are assigned.
   - Use `HasValue`, `HasPublicId`, `IsFullyAssigned` to check state.

7. **Single Roundtrip for Batch Operations**
   - Use PostgreSQL `JSONB_TO_RECORDSET` with a single CTE that handles both `UPDATE` and `INSERT`.
   - The CTE locks rows with `FOR UPDATE`, applies the modifications, and returns the **old** balance (from the locked row) for existing records, or the **default initial values** (e.g., 0) for new records.
   - Return `ResolvedId`, `Balance` (old or default), and `ResolvedPublicId` for each row.
   - Build dictionaries (`resultDictById`, `resultDictByPublicId`) to map results back to aggregate states.

8. **No Implicit Commit**
   - The use case creates a transaction (e.g., `DapperAccountsBatchTransaction`), calls business methods on aggregates, then explicitly calls `Run()`.
   - The transaction tracks all modified states via its own internal `_registeredStates` list.

9. **Resource Ownership**
   - The transaction class is fully responsible for opening and closing the database connection, as well as managing the transaction.
   - The connection and transaction **must not** be stored as fields across the entire lifetime of the object; they are created and disposed inside the `Run` method.
   - `IAsyncDisposable` must follow the standard pattern with a `protected virtual DisposeAsync(bool disposing)` method, allowing derived classes to clean up their own resources without breaking the base class disposal.
   - Inside `Run`, a `finally` block must ensure that the connection (obtained from the transaction object) is disposed, even if an exception occurs.
   - The only responsibility of `IAsyncDisposable` base implementation is to denote the one-time nature of `DapperAccountsBatchTransaction` lifecycle.
   - The `Run` method must have a parameter CancellationToken cancellationToken = default. Use this token for all async operations inside (`BeginTransaction`, `Commit`, `Rollback`).

---

## Technical Implementation Details

### Data Access Layer (Dapper + Npgsql)

- **PostgreSQL** is the target database.
- **Dapper** is used for micro‑ORM mapping.
- **Npgsql** provides the ADO.NET driver with advanced features like **JSONB** support and **DataSource** management.

#### JSONB Serialization

- Instead of building JSON strings manually, we use **`System.Text.Json`** source generation to serialize internal structs.
- The struct `NpgsqlDynamicJsonAccountChanges` is marked `partial` and contains `internal` fields with `[JsonInclude]` and `JsonPropertyName` attributes.
- A nested class `GeneratedForInternalParentRequiringPartialAndJsonIncludeOnInternalMembers` inherits from `JsonSerializerContext` and is decorated with `[JsonSerializable]`. This enables Roslyn source generation, making the internal fields visible to the serializer without exposing them publicly.
- The `partial` struct and the context class work together: the context is used by Npgsql when `EnableDynamicJson()` is called on the `NpgsqlDataSourceBuilder`.

#### SQL Generation and Parameterization

- All SQL is generated in the `NpgsqlAccountSqlHelper` static class.
- Two main entry points:
  - `BuildJsonbChangesArraySourceSql` – for regular mode (updating known states). It builds an array of changes and adds a single `@Jsonb` parameter.
  - `BuildJsonbQuerySourceSql` – for batch mode (querying based on specifications). It builds a query that returns a JSONB array using `jsonb_agg(to_jsonb(r))`.
- Parameters are added directly to an `NpgsqlParameterCollection` (not `DynamicParameters`) to avoid boxing and improve performance.
- The helper methods `AddCondition` and `AddAction` (or `SetActionProperties`) add parameters to the collection with explicit `NpgsqlDbType`.

#### SQL Builder Abstraction

- SQL generation logic is extracted from the transaction class into a separate interface `IDapperAccountSqlBuilder`.
- The interface defines two methods:
  - one for explicit ID mode (`states`),
  - one for batch mode (`batchConditions`, `batchActions`, `batchSize`, `useForUpdateSkipLocked`).
- The transaction class receives the builder via a `protected abstract` property.
- In `AddParameters`, the `CommandText` **must always be cleared** (set to an empty string) before calling the builder, to avoid reusing stale SQL from previous calls.
- After the builder call, the code must verify that `CommandText` is not empty; if it is, throw an `ApplicationException` with the exact predicate `"string.IsNullOrEmpty(command.CommandText)"`.

#### Implementation of `SqlMapper.IDynamicParameters`

- The `DapperAccountsBatchTransaction` class implements `SqlMapper.IDynamicParameters`.
- In the `AddParameters` method, the entire SQL command is built and all parameters are added to the `NpgsqlCommand`.
- The `Run` method calls `QueryAsync` with a placeholder SQL (`"see AddParameters 👇"`) and passes `this` as the parameter object. **To support cancellation, the call must use `CommandDefinition` with the `cancellationToken` parameter.** Dapper then invokes `AddParameters` to populate the command.

#### CTE Structure (PostgreSQL)

- The main CTE (`BuildFullCte`) consists of:
  - `jsonb_source` – the source of the JSONB data (either from `@Jsonb` parameter or from a subquery).
  - `records_with_changes` – parses the JSONB using `JSONB_TO_RECORDSET`, joins with `account_public_id` to resolve existing `account_id`.
  - `lock_records_to_update_wo_deadlocks` – locks the rows using `FOR UPDATE OF` and orders by `locked.id` to prevent deadlocks.
  - `updated_records` – performs the UPDATE, using `CASE` to apply balance changes (absolute, increment, set).
  - `inserted_public_ids` – inserts new public IDs for new accounts.
  - `inserted_records` – inserts new account rows.
  - Final SELECT combines existing and new records, returning `ResolvedId`, `Balance` (the **old** balance from the locked snapshot for existing rows, or the **default initial value** for new rows), and `ResolvedPublicId`.
- **Callback timing**: The CTE executes the `UPDATE`/`INSERT` before the aggregate's callback is invoked. The callback is triggered after the results are parsed. For existing records, it receives the old balance via `LockedRecordData`; for new records, it receives the default initial state (e.g., `Balance = 0`). The aggregate performs validation uniformly and may throw an exception to roll back the transaction. The aggregate does **not** differentiate between new and existing records.

### Exception Handling

- **Domain exceptions** are represented by a hierarchy starting from `AccountException` (abstract, with private constructor).
  - Concrete exceptions are nested sealed classes with `internal` constructors.
  - They carry payload via read‑only auto‑properties.
  - They are used for business‑rule violations that the UI/API should handle (e.g., `ConcurrentBalanceModification`).
- **Runtime invariants** (programmer mistakes) use `ApplicationException` directly, never subclassed.
  - The exception message is the exact predicate that failed (e.g., `"_accounts.Count != _registeredStates.Count"`).
  - They are thrown with `if (condition) throw new ApplicationException("condition")` – no helper methods, with a message that exactly duplicates the checked condition as a code string. This allows instant identification of the root cause from the exception text without additional analysis.
- **`ArgumentOutOfRangeException`** is used for method parameter validation, always with `message: null`.
- **Protect against hidden NullReferenceException**.
  - Methods must check reference parameters for null and throw `ArgumentNullException` with the parameter name. This rule is not delegated to the caller, because the interface contract does not guarantee non‑null at runtime itself. Exactly these input assertions define the contract.
  - Properties behind the interfaces (e.g., `IDbTransaction.Connection`) are not guaranteed to be non‑null at runtime, even if the object was obtained via a correct API. Before using such a property, always check it and throw `ApplicationException` with a message that exactly states the condition (e.g., "transaction.Connection == null"). This ensures fail‑fast and a clear indication of the violated invariant. Prefer extracting extra variable with coalescing to improve readability.

### Code Style and Formatting

- **Line length**: Aim for 120 characters, but it’s acceptable if the exception type fits entirely on one line; the message may be longer.
- **Argument wrapping**: When wrapping method arguments: place each argument on a new line, indented by 4 spaces from the method name; put the closing parenthesis on the same line as the last argument.
- **`=>` operator**: For simple expression‑bodied members, place `=>` on a new line if the expression is long (e.g., `internal DapperAccountState(DapperAccountId identity, EventHandler callback) => ...`).
- **Single‑line loops**: For short loops (like invoking callbacks), use `for (int i = 0; i < count; i++) statement;` on one line.
- **Null‑coalescing**: Prefer `_value ?? throw new ApplicationException("!_value.HasValue")` over separate null checks.
- **Local variables**: Use `var` where the type is obvious; but for clarity, sometimes explicit types are used.
- **Constants**: `internal const int MaxBatchSize = 50;` and use it as default for `BatchSize`.
- **Use `Array.Empty<T>()`** instead of `new List<T>()` for empty collections.
- **Always specify `message: null`** in `ArgumentOutOfRangeException`.
- **Passing parameters from literals**: always explicitly specify parameter name in method invocations where the parameter expression doesn't match the parameter name.
- **Nullability is disabled (no #nullable enable)**: no ? or ! in signatures.
- **Minimal diff principle**: do not reformat code not directly affected by logic changes.
- **Preserve existing indentation and blank lines**: especially around loops, blocks or multiline member declarations. Do not remove blank lines around foreach and inside blocks unless they are part of the changed logic, they were intentionally added to improve code readability.
- **One‑liner loops are allowed when line width permits**: keep them for visual pairing of adjacent operations.
- **Place new members according the layout and split groups**: const -> static readonly fields -> readonly fields -> non-readonly fields -> auto-properties -> ctor -> protected abstract or one-liner-virtual (after ctor or chronologically in complex cases) -> non-private methods (chronologically outer) -> private (invokers upper invokees lower + chronologically inner + more important/first-gen-invokees first) -> nested types (chronologically prefer non-private first). Always leave empty line between the groups.
- **Internal mid-file recognition aid**: any internal type must avoid declaring public members unless used as part of implicit interface implementation.
- **Use block-scoped namespaces (namespace Name { ... })**: instead of file-scoped namespaces (namespace Name;).
- **Preservation of Marker Literals**: string literals containing emojis (👇, 👉, ⚠️, ✅, etc.) or other unique markers are an integral part of the code. They serve debugging, visual highlighting, or as placeholders in SQL queries. Such literals must not be changed, removed, or replaced during refactoring, as their loss would break its purpose (e.g., calling `connection.QueryAsync(commandText: "see AddParameters 👇"...)`).
- **Refactoring and Context Preservation**: when modifying existing code (especially during refactoring), all existing constructs, idioms, naming styles, and operator sequences (e.g., using, await using, checks, literals) must be kept unchanged unless they are the direct subject of the change. This applies to both semantics and syntactic formatting (e.g., presence of emojis, order of checks). Deviations are permitted only when explicitly stated in the requirements.

#### Markdown soft line wrapping rules (by the markdown two trailing spaces)

**The rules are demonstrated by the definition itself:**

- Target line length cap: 72 characters.
- Wrapping: only wrap when the target line length exceeds 72 characters.
- Break points: natural phrase breaks, but never break a line that  
  already fits.
- Exempt trailing spaces before a blank line, a heading, a list item  
  (any marker: -, *, +, or number), a table row, or a code fence.
- Exempt headings, tables, and code blocks completely from wrapping.
- **Length counting**: count every character in the raw Markdown source  
  line, including letters, numbers, punctuation, spaces, backticks,  
  asterisks, underscores, and any other formatting symbols. The target  
  length of 72 characters applies to this full line. Do not exclude  
  any characters for counting purposes.
- **No arbitrary breaks**: do not break lines that already fit within  
  72 characters. Wrap only when necessary, using the minimum number  
  of lines required.

Here is an example of a paragraph. This may consit of multiple  
sentences (as you can see the previous word didn't fit entirely, so  
it was soft breaked).

### Project Structure

The example is split into three projects:

1. **YourCompany** – domain model, specifications, and exceptions.
2. **YourCompany.Dapper** – infrastructure adapters (Dapper, Npgsql, CTE helpers).
3. **YourCompany.ConsoleProgram** – entry point and use cases.

Key files:

| File | Purpose |
|------|---------|
| `YourCompany/Account.cs` | Pure domain aggregate. |
| `YourCompany/AccountException.cs` | Domain exception hierarchy. |
| `YourCompany/Persistence/IAccountRecordData.cs` | Record data contract. |
| `YourCompany/Persistence/AccountSpecifications.cs` | Specifications as `EventArgs`. |
| `YourCompany.Dapper/DapperAccountId.cs` | Two‑phase identity implementation. |
| `YourCompany.Dapper/DapperAccountRecordData.cs` | Mutable record data for Dapper mapping. |
| `YourCompany.Dapper/DapperAccountState.cs` | Implementation of `IState` and `IStateModifying`. |
| `YourCompany.Dapper/DapperAccountsBatchTransaction.cs` | "Unit of work" with `IDynamicParameters`. Abstract base transaction class; implements `IDynamicParameters` and delegates SQL construction to the builder. |
| `YourCompany.Dapper/IDapperAccountSqlBuilder.cs` | SQL builder interface. |
| `YourCompany.Dapper.PostgreSQL/NpgsqlAccountsBatchTransaction.cs` | PostgreSQL-specific implementation; provides connection and builder to the base. |
| `YourCompany.Dapper.PostgreSQL/NpgsqlAccountSqlHelper.cs` | Helper for SQL generation (moved to PostgreSQL assembly). |
| `YourCompany.Dapper.PostgreSQL/NpgsqlDynamicJsonAccountChanges.cs` | DTO for JSONB (moved to PostgreSQL assembly). |
| `YourCompany.ConsoleProgram/Program.cs` | Entry point with test data seeding. |
| `YourCompany.ConsoleProgram/UseCases/TransferFunds.cs` | Use case #1. |
| `YourCompany.ConsoleProgram/UseCases/GiveBagelsToPoorAccounts.cs` | Use case #2. |

---

## Building a New Feature: Step‑by‑Step Checklist

1. **Define record data interface** (e.g., `IProductRecordData`) with read‑only properties.
2. **Create mutable implementation** (e.g., `DapperProductRecordData`) for Dapper mapping, with internal `ResolvedId`, `ResolvedPublicId`.
3. **Create concrete `Identity`** (e.g., `ProductId`) with two‑phase assignment if needed.
4. **Define specifications** (inherit `EventArgs` + implement `ISpecification<T>`).
5. **Implement aggregate** with business methods, using extension methods from `StateExtensions` (e.g., `GetFinishedAccessData`, `GetSettingPropertiesBeforeDataChanging`, `CheckHasSingleSpecification…`). The aggregate should never differentiate between new and existing records – it works solely with the provided state.
6. **Implement `DapperProductState`** (or reuse `DapperAccountState` pattern) that implements both `IState` and `IStateModifying`.
7. **Extend `DapperAccountsBatchTransaction`** or create a similar class for the new aggregate, implementing `IDynamicParameters` and using `NpgsqlProductSqlHelper`.
8. **Create SQL helper** that builds the CTE and adds parameters to `NpgsqlParameterCollection`.
9. **Create JSONB DTO** (`NpgsqlDynamicProductChanges`) as a `partial struct` with `[JsonInclude]` and a `JsonSerializerContext`.
10. **Implement use case** that creates the batch transaction, builds aggregates via `ById` or batch specifications, and calls `Run()`.
11. **Test** thoroughly; ensure no separate SELECT before the CTE, and that all exceptions follow the domain/runtime separation.

---

## Common Pitfalls (Lessons Learned)

- **Never use `EnsureWasAssigned()` in getters** – prefer null‑coalescing throw with a meaningful predicate.
- **Do not store `_callback` in the aggregate as a field** – it should be passed via `Claim` and only used in `OnTransactionCallback`.
- **When adding specifications, ensure they are `EventArgs` if they need to be processed in the aggregate’s callback.**
- **Always check that `_registeredStates` is not null before accessing it.**
- **Avoid `SingleOrDefault()` in a loop** – it’s expensive; use `for` and check types explicitly.
- **Use `COALESCE` in SQL for null handling, never in C# if you can avoid it.**
- **Remember that `ArgumentOutOfRangeException` should always have `message: null`.**
- **For `ApplicationException`, the message must be exactly the predicate that failed.** – e.g. `if (condition) throw new ApplicationException("condition")`
- **The `UseForUpdateSkipLocked` flag is set in `ConfigureBatch`, not in `ById`.**
- **The placeholder string passed to `QueryAsync` (e.g., `"see AddParameters 👇 "`) must never be changed** - because the actual SQL is set inside `AddParameters`. This is a technical necessity due to Dapper’s requirements.
- **When overriding `DisposeAsync` in a derived class, do not add an empty override if the derived class has no additional resources** — rely on the base implementation.
- **The SQL stub in `Run` is: "see AddParameters 👇"** — do not remove or alter the emoji.
- **The two calls `OnAfterRecordDataLocking` and `OnAfterDataAccess` are invoked in sequential one‑liner for‑loops** — placed directly under each other to visually link them. Do not expand to multi‑line.
- **Instead of casting to make code compile (e.g., `_batchConditions ?? Array.Empty` -> List vs Array, etc.), assign to a local variable before use** — Do not inline with null‑coalescing (??) incompatible types.
- **Retain original signatures when unchanged** — e.g. `IStateAccess` is a **generic** type with `TRecordData` parameter. The name can be shorten for brevity in the docs, but have to remain original in the code.
- **await using var _ = this;**  — at the start of that method serves as automatic DisposeAsync call on method exit (even on exception), preventing further use of the class (e.g. `DapperAccountsBatchTransaction.Run` method) by fail‑fast on repeated calls via the _disposed flag, and providing explicit semantic designation of single‑use objects.
- **Non-private group = all members except private** — methods within this group are ordered by call chronology: entry points first, then the methods they invoke or followed by, regardless of access modifier (public, internal, protected, etc.).

---

## Final Reminder

This context is a **living document**. As you implement new features, you may discover missing pieces. When that happens:
- Either extend the current implementation following the same architectural rules, or
- Consider switching to a more complete entry point branch if the missing feature is already implemented there.

**The original repository’s commit history is the ultimate source of truth.** Use `git log` and branch graphs to understand how the architecture evolved.

Happy building! 🚀
