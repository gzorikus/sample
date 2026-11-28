## When a tool is written

**Write to this contract. Report every difference — run the reference  
before writing.**

**Keep a tool in the harness's durable folder, never in a temporary  
one** — a temp folder is wiped and the map names a dead file.

**Write a tool, never take one.** Write it from the convention: no  
literals, no mark, in whatever the harness runs. Take the reference's  
script as it stands only where the roles are those four, in that order  
— and then whole: the typed array, the literals and the byte-order  
mark stay; the BOM check exempts that file. Allow it only on the  
owner's word; never let the shell imply it. A repository with no code  
copies nothing.

Treat the three tools as contracts serving the README and file  
hygiene: behaviour fixed here, implementation in the harness; report a  
different ready-made script, never copy it. Create a log tool only  
where the README's answers put it. Ride a tool's answers together: the  
mask, the excludes, the row unit.

**The commit-log tool**  
([`CommitLogsToReadmeMd.ps1`](../scripts/CommitLogsToReadmeMd.ps1))  
writes every counted commit's block into the root `README.md`,  
verbatim.

- Read `--date-order --reverse`; merge and `fixup!`/`amend!`/`squash!`  
  excluded, folded into their targets. Carry each commit's stat lines  
  along.
- A block: `### Commit: <subject>`, the stat table by directory with  
  diff bars that cannot fold, the body, then  
  `<!-- ### Commit: <subject> END -->`.
- **Replace, never duplicate**: replace the same subject whole, fill a  
  one-line placeholder, append a missing block after the last. The  
  log's order is the document's order.
- `init` and nothing else creates the root `README.md`. No file: stop  
  and name it. Check `commit.cleanup = verbatim` once per repository.

**The table tool**  
([`CommitsTableToReadmeMd.ps1`](../scripts/CommitsTableToReadmeMd.ps1))
regenerates one entry-point branch's block, entered from the break  
alias, not a shell.

- Read the live todo first. Outside a rebase, or with no branch in the  
  todo, return without writing.
- Take the branch from the todo's `update-ref` line. Never guess it.
- Count per commit: the link to its block, the example READMEs it  
  touches, the modules it moved and their line balance. **Check the  
  balance against the mask's full count before writing** — a wrong sum  
  stops the run.
- The block: `### Branch: <name>` … `<!-- ### Branch: <name> END -->`.  
  Fill a placeholder whole; an existing block keeps its hand-written  
  Pros and Cons and regenerates only the derived parts.
- **Give the trunk no block**: a break on the trunk returns. A missing  
  branch block is the loudest failure; the reference exempts only the  
  trunk; more exemptions are the project's answer.

**The BOM check** ([`ControlNoBom.ps1`](../scripts/ControlNoBom.ps1))  
prints every file whose first three bytes are a UTF-8 BOM, excluding  
the generated-code folders.

- **One exemption**: a script taken as it stands keeps its mark for  
  the emoji literals.
- **Check, never repair**: the list is the defect list, empty output  
  is clean, rewrite nothing. Extra excludes are the mask's answer, the  
  same list the other two tools carry.
