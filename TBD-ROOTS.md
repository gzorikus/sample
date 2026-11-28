# TBD-Roots

The project's convention. The method is `ai/`; this file answers its  
questionnaire and nothing else.

## Purpose

An educational corpus and an open-source boilerplate: enterprise  
components assembled so a team owns its framework instead of  
renting it. Read from `README.md`, `## Goals`.

## Orientation

Public. Trunk `public`. Licence MIT, © 2026 Georgii Kalashnikov;  
source headers carry no copyright line.

## Roles

Canonical order; lower-case names index for index; a role is the  
nature of the knowledge a commit carries, nothing else chooses it.

| role | name | one phrase | place |
|---|---|---|---|
| 🏠 | domainman | requires tools that speak business over the code; forms the owned framework and develops domain modules | `*.UseCases*`, the `YourCompany.OLTP.StateOwnership` core |
| 🧱 | facademan | assembles domain modules with ASP.NET, OpenAPI, Configuration, Reflection, Serialization, DI; exposes APIs and client stacks | `*.DI*`, `YourCompany.Configuration`, `YourCompany.Reflection` |
| 💾 | persistman | speaks database over the code; adopts, optimises and upgrades EF Core and other data-access modules | `*.Persistence*`, `*.EFCore*` |
| 📦 | streamman | organises data delivery and analysis — broker, MassTransit, outbox — cross-cutting concerns, logging, scheduling | `*.EventsProducing*`; no folder yet |

Hand-over runs 🧱 (0) → 🏠 (1) → 💾 (2) → 📦 (when streaming  
arrives), as the history already does: one role writes its place,  
the next reads what it left.

## Places

The root holds `ai/` (the method), `examples/`, `scripts/`, `src/`,  
`.github/`, `.vscode/`. A role's place inside `src/` is its module  
suffix, not a folder of its own; folders are shared where knowledge  
is — the module `YourCompany.OLTP.RecordsManagement`, `sample.slnx`  
and `README.md` are touched by every role, and that is the project's  
answer. A folder states itself in one `README.md` at its root; a  
folder with nothing to hand over has none.

## History

The root is `init` (`f4b778c`): `.gitattributes`, `.gitignore`,  
`LICENSE`, `README.md`; no role and no iteration, and  
`0|🧱: welcome aboard` stands beneath it. The convention rides  
`init` as a `fixup! init`, and `init` is rewritable: a new edition  
folds into a new root and closes every open `PR/`.

Folders are named by module — `YourCompany.*`. A version of their  
content is named by a flat tag `{branch}-{iteration}`, one per  
branch per iteration; only the trunk's tag moves.

## Iteration

The todo is `bonus-last-git-rebase-todo` at the root, kept in the  
tree; its standard head is preserved byte for byte. An iteration  
opens with `# Startup Iteration N. {goal}` — the goal alone is  
written by hand, and the `pick`s arrive with the first `PR/`. The  
text may propose switching an iteration when its goal is closed.

## README

The root `README.md` is living. `## Commit logs` is written by  
`scripts/CommitLogsToReadmeMd.ps1` and the tables by  
`scripts/CommitsTableToReadmeMd.ps1`. Every entry-point branch has  
a block under `## Entry points`; the trunk has none. A row is a  
commit; the columns are `Log`, `Examples`, `Modules`; the block  
head keeps `Pros:` and `Cons:`. The line-count metric leaves out  
`examples`. `interactiverebasebeforebreak` stands in `.gitconfig`  
at the root; a clone applies it once, and  
`commit.cleanup = verbatim` rides with it.

```
git config --local include.path ../.gitconfig
```

## Graph

Wanted. It stands in `## Current solution`, redrawn as iterations  
close.

## Wrap

Taken: a text is wrapped to 72 by `ai/TBD-ROOTS - wrap a text.md`.  
The law is refused only where the log is refused; this log is kept.

## Routes

1. `src/` — a change lands on the entry-point branch as  
`{N}|{role}: {subject}`, the why in the body; then the log tables  
and the root edition are rebuilt.
2. `examples/` — one example per commit, with its own `README.md`.
3. `scripts/` — a tool changes in the same commit as the artefact  
it serves.
4. `ai/` — a clarification is written into the prompt that owns it,  
in the same turn.
5. `README.md` — generated sections come from their script; a  
hand-written section changes by its own commit.
