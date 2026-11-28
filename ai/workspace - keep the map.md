# Keep the map

**Open when** a run starts; anything is compiled, re-compiled or found  
absent.

Follow the deploy's order, 0-8. Treat this file as the map: fields,  
lint, rules.

Treat the method package as no area: its record is its commit. Open  
the gate's own entry by hand, once. Name in a `pending` entry the  
mechanism that will settle it; keep `mechanisms: —` for  
`not applicable` alone. Leave no area the run touched without its  
entry. A map read and never written is ballast: the run carrying it is  
the defect.

```
## <id>
id: <slug>                     area: <one phrase>            consumer: <what reads it>
deps: <id>[, <id>] | —         trigger: <event that makes it needed>
acceptance: <command>; expect: <result>[; then <command>; expect: <result>]
mechanisms: <path> sha256:<hex>[, <path> sha256:<hex>] | — (only `not applicable`)
automation: code | assisted | manual          cost: <n> — <what a run must read> | 0
witness: <command and verdict> @<date> | —    worker: main | sub:<id>
state: pending | in progress | settled | not applicable | blocked on <id>
stale: no | artifact | spec <file>:<section> | history  blast: <what is re-derived when it lands>
remarks: - <date> <who>: <what was expected — what happened>
```

- Keep one file per workspace, in the harness, never in the  
  repository, named `deploy-map.md`: `last_reconciled`, `workspace`,  
  the bound repositories with their trunks and areas.
- Ids are slugs. Never rename one.
- Cite the project in the map, never the method: point an entry at its  
  mechanism or the convention's section, or keep it `pending` with  
  what would close it. A path into the method package is a defect.
- Treat an entry with no consumer as decoration, with no acceptance as  
  opinion. Never `settle` an entry with no mechanism; name in a  
  `pending` entry the command that will settle it.
- Run `code`; its output is the witness. Run `assisted` with a  
  person's parameters. Read `manual` each time — a debt, never a home.  
  Pay the budget down most expensive first; show the old and the new  
  number. Treat a mechanism that costs more than the reading it  
  replaces as a regression.
- Make a witness what another person can repeat. No witness:  
  `pending`.
- Read `pending` as coverage, not a plan: what is not covered, never  
  what to do. Settle an entry that cannot be current by design, saying  
  so; never mark it `stale`.
- Treat staleness as a flag; a rule's change travels down `deps`. A  
  stale or blocked area that cannot be rebuilt when touched: stop,  
  name the entry, the reason, the gap.
- Treat history as the ground: a landing that moves the trunk makes  
  the entry `stale: history` until reconciled. Treat a remark as a  
  trace, never a lesson: what bites twice becomes a rule in the  
  convention.
- Let the background pass reconcile only: witnesses, orphan  
  mechanisms, appeared consumers, staleness, `last_reconciled`, one  
  line of movement, the fetch. It never edits, commits, settles or  
  invents.
- Let the map lint itself as entry `map`: fields present, `deps`  
  exist, no cycles, `settled` has mechanisms and a witness,  
  `not applicable` has a reason, `cost` is a number, 0 only for  
  `code`.
- Render a dashboard from the map: counts, the code share, the budget,  
  staleness, what is not settled and what it waits on. It names no  
  next step, has no state, is regenerated. A status without its  
  mechanism is a lie.
- Send git the specimen, not the map: a rule's change is its own  
  commit, naming the entry and the reason.
- Land no half-entry: `settled` with a witness, or `pending` with what  
  would settle it. Three honest exits: a skeleton map and what is  
  next; that plus the entry document and the findings; that plus  
  answers and compiled mechanisms.

Collect in parallel; never land in parallel. Land one item at a time:  
index, order, `fixup` targets; two writers in one file make a third  
result. Make independence by file, not only by `deps`: land items  
touching one file in turn.

Treat a session as no unit of work; the item is. Take one item per  
run: land it, write its witness, end; the next run reads the map,  
never the previous chat. A run that finds its item taken does not  
wait: it takes another, or leaves that one and says so. Lock nothing,  
queue nothing: the map is the handover.

Treat the coordinator as no author: it holds the map and the  
boundaries, and re-runs the acceptance itself instead of trusting a  
report. Never block a run's own word by another's work: an assertion  
lands as a `fixup` on its home or as its own commit; it never  
overwrites another's text.
