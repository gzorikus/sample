# DeepSeek Aggregate State Transition 🤖

The example is fully suggested and implemented by DeepSeek.

## Commit: 1|🏠: the heart of OLTP is transaction

This is a complete implementation for two artificial use-cases:

- **bank transfer** - two‑account debit/credit;
- **give bagels** - matching accounts balance increment.

We're using the minimal entry point of the repository:  
`oltp-basic-object-model-the-rest-is-up-to-you`.

### What You’ll Find Here 🚀

- **Pure domain aggregate** – no persistence, no infrastructure.
- **Delegated state access & modification** – implementing  
  `IStateAccess` / `IStateModifying`.
- **PostgreSQL + Dapper** – perform a **single DB roundtrip**  
  using CTE with `JSONB_TO_RECORDSET` and `jsonb_agg`.
- **Two‑phase `Identity`** – `PublicId` (Guid) and `Value` (long PK)  
  combined for different state access patterns.
- **Specifications as `EventArgs`** – carrying both intent and  
  callback triggers.

### How to Run ⚙️

1. Create a PostgreSQL schema `deepseek-aggregate-state-transition`:

   ```sql
   CREATE SCHEMA "deepseek-aggregate-state-transition" AUTHORIZATION postgres;
   GRANT ALL ON SCHEMA "deepseek-aggregate-state-transition" TO postgres;
   GRANT USAGE ON SCHEMA "deepseek-aggregate-state-transition" TO "YourCompanyDemoUser";

   GRANT SELECT, INSERT, UPDATE, DELETE
       ON ALL TABLES IN SCHEMA "deepseek-aggregate-state-transition" TO "YourCompanyDemoUser";

   ALTER DEFAULT PRIVILEGES IN SCHEMA "deepseek-aggregate-state-transition"
       GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO "YourCompanyDemoUser";
   ```

2. Create tables within that schema:

   ```sql
   CREATE TABLE account_public_id (
       account_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
       public_id UUID NOT NULL,
       CONSTRAINT uq_account_public_id UNIQUE (public_id));

   CREATE TABLE account (
       id BIGINT NOT NULL PRIMARY KEY,
       balance NUMERIC(19,4) NOT NULL DEFAULT 0,
       last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
       CONSTRAINT fk_account_public_id FOREIGN KEY (id)
           REFERENCES account_public_id(account_id) ON DELETE RESTRICT);

   -- !! Do not hardcode 10 in production – use a computed column or business flag. See GiveBagelsToPoorAccounts.cs
   CREATE INDEX ix_account_poor ON account (balance) WHERE balance < 10;
   ```

3. Run the project. Expect to see the console output:

   ```plaintext
   Transfer succeeded
   Bagels given to 1 poor accounts.
   ```

### Where to Learn More 📖

- **For a narrative walkthrough** (the essay):  
  [*The Aggregate as a State Transition Function*](../../ai/DeepSeek/State%20Ownership%20Pattern%20in%20OLTP%20PostgreSQL%20-%20Building%20an%20Aggregate%20as%20a%20Closure%20Over%20State%20with%20No%20Read‑Before‑Write/The%20Aggregate%20as%20a%20State%20Transition%20Function.md)

  It explains:

  - Why there is **no SELECT before UPDATE**.
  - How the aggregate remains completely infrastructure‑agnostic.
  - The trade‑offs of this entry point vs. higher‑level branches (EF  
    Core, batch handling, etc.).
  - How to extend the example with **read‑only transactions**,  
    **trivial setters**, or **alternative persistence stacks** without  
    touching business logic.

- **For a structured, top‑down alternative** (the reference hub):  
  [*State Ownership Pattern WIKI*](../../ai/DeepSeek/State%20Ownership%20Pattern%20in%20OLTP%20PostgreSQL%20-%20Building%20an%20Aggregate%20as%20a%20Closure%20Over%20State%20with%20No%20Read‑Before‑Write/WIKI.md)  
  — browse the annotated table of contents, per‑interface  
  specifications, module breakdown, and commit‑by‑commit rationale  
  without the long‑form narrative flow.

### Key Files 📁

| File | Purpose |
|------|---------|
| [`IAccountRecordData.cs`](src/YourCompany/Persistence/IAccountRecordData.cs) | Read‑only data contract for the aggregate. |
| [`AccountSpecifications.cs`](src/YourCompany/Persistence/AccountSpecifications.cs) | Compact data specifications (intent by `Triggering : EventArgs`). |
| [`Account.cs`](src/YourCompany/Account.cs) | Pure domain aggregate. |
| [`DapperAccountId.cs`](src/YourCompany.Dapper/DapperAccountId.cs) | Two‑phase identity (Guid + long). |
| [`DapperAccountRecordData.cs`](src/YourCompany.Dapper/DapperAccountRecordData.cs) | Mutable infrastructure implementation. |
| [`DapperAccountsBatchTransaction.cs`](src/YourCompany.Dapper/DapperAccountsBatchTransaction.cs) | Unit of work for OLTP batch operations. |
| [`NpgsqlAccountSqlHelper.cs`](src/YourCompany.Dapper/NpgsqlAccountSqlHelper.cs) | Mass CTE helper for Account operations. |
| [`TransferFunds.cs`](src/YourCompany.ConsoleProgram/UseCases/TransferFunds.cs) | Use case 1 (orchestration simplified). |
| [`GiveBagelsToPoorAccounts.cs`](src/YourCompany.ConsoleProgram/UseCases/GiveBagelsToPoorAccounts.cs) | Use case 2 (timer‑like simplified). |
| [`Program.cs`](src/YourCompany.ConsoleProgram/Program.cs) | Entry point with data populating. |
