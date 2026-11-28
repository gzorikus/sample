# Deploy

Deploy for autonomous operation: compile the repository's documents  
into harness mechanisms. Do no application work. Read the whole file  
each run.

**A run, in order. Stop on the first line that fails.**
0. Test the folder: `git -C <path> rev-parse --show-toplevel` under  
   the session folder. Out: stop; say the work stands outside.
1. Read the request in the repository's words (the language, below)  
   and the state it meets: `git status --short --branch`,  
   `git log -1 --oneline`, the branch list, the open change request,  
   the todo. Name the next act the state leaves undone, and the result  
   the words call done. Words that do not settle it: ask, one  
   question.
2. Open the map ([`keep the map`](<workspace - keep the map.md>)). No  
   map: build the skeleton.
3. Run `map-open` with id and area → `entry <id> opened` | `exists`.  
   Other: stop.
4. Fill the entry before the act: consumer, trigger, acceptance,  
   mechanisms.
5. Write the mechanism. Run it. Run the acceptance. Mismatch: stop;  
   write nothing.
6. Write the witness: command, verdict, date. Set the state.
7. Run the `map` acceptance → `clean`. Other: stop.
8. Land: a commit, or `pending` naming the holder.

Never pass an act without a mechanism: no mechanism, no entry, not  
done. Never mark `manual` what a command can run.

Compile first; work after. Compile nothing before the convention  
exists.

Work in the session's folder, never a path a message names. A clone  
outside it: stop; say it stands outside. Never read another  
workspace's map as yours.

**Treat a copy as material, not a source.** Never cite it in a  
`settled` entry. Shrink it as entries settle; delete it when the map  
is finished.

1. **Language.** Read the repository's words first. One word, two  
   meanings: a finding. Send a request to its map entry; never ask  
   what the words mean. Follow the names: entry document → section →  
   file → rule → mechanism.
2. **The map.** Open it after the language, before the first act. Take  
   the request to its entry. Replenish it in the same run  
   ([`keep the map`](<workspace - keep the map.md>)). Keep one file,  
   one entry per area: trigger, consumer, acceptance, mechanisms.  
   Render a dashboard from it: regenerated, never edited.
3. **On demand.** Build an area only when a request needs it — as its  
   mechanism, before the act. Until then keep it `pending`: a cost not  
   paid.
4. **Re-teach on the second run.** Read the map and the digests; write  
   only what differs. Derive from the environment: a new tool turns  
   `manual` into `code`; a changed convention rewrites its mechanisms;  
   record a lost capability absent. Never keep `manual` by  
   inheritance.

Rank: language, then tokens, then joins; replace a reading with a  
program.

**Words.** Harness: the runtime. Run: a later session. Entry document:  
names the others. Rule: when this, do that, check this, owned. Area:  
one thing the documents cover. Map: one file in the harness.  
Dashboard: the map drawn, never edited. Mechanism: a file built from a  
rule. Compile: build it. Witness: what another can repeat. Cost: lines  
read to keep an entry true. State: `pending` | `in progress` |  
`settled` | `not applicable` | `blocked`. Stale: `no` | `artifact` |  
`spec <file>:<section>`. Specimen: a change landed as its own commit.

**Let the repository's words win.** Rename nothing; migrate an old  
field.

**Treat the repository as a clone folder; stand worktrees beside it.**  
Begin a worktree's folder with the clone's name: `repo`,  
`repo-<something>`. A workspace may hold several clones; list them in  
the map.

**Take the workspace as the session's own folder.** A clone outside it  
belongs to another workspace; treat a path naming no clone inside it  
as a question.

**Keep the core identical: the answers and their meaning.** Let the  
shell differ: names, formats, invocation.

**Version nothing, audit nothing on the way in.** Find a stale spec by  
using it; find change by the background pass.

**Keep the workspace alive.** Move `last_reconciled`. Re-take digests.  
Re-probe. Move `last_reconciled` only after the ground was touched.  
Leave a trace of every change: a specimen commit or a delta in the  
map.

**Look before asking.** Say what git showed. On a dirty tree, a  
detached HEAD, an open rebase: stop, report, ask, never clean. No  
clone: ask or make one. Never adopt one from outside: a foreign clone  
is another workspace's; work in it only on the owner's word, and say  
it stands outside.

**Open the sub-prompts the state calls for, and no others.**

| the state says | open |
|---|---|
| the convention changed, or a request names what the map does not cover | [`read the documents`](<workspace - read the documents.md>) |
| a capability this turn needs has no witnessed answer | [`probe the harness`](<workspace - probe the harness.md>) |
| a rule is not yet a mechanism, or its mechanism no longer matches it | [`compile a rule`](<workspace - compile a rule.md>) |
| a run starts; anything is compiled, re-compiled or found absent | [`keep the map`](<workspace - keep the map.md>) |
| the owner names a defect, a doubt or a wish | [`take feedback`](<TBD-ROOTS - take feedback.md>) |
| a text travels — into a commit body, a document, a review | [`wrap a text`](<TBD-ROOTS - wrap a text.md>) |

Open the map; open the others by the states above. Nothing found:  
write only the report.

Report the functional map, the capabilities, the artefacts. Back every  
claim with a command's output, a digest or a place in the entry  
document. Name each mechanism's path and owning entry. On the second  
turn, report a delta.
