## When commits are made

**Commit in the order the todo gives; replay only the breaks it  
names.**

**Turn a sequence typed twice into a script.** Collect the replay, the  
commit assembly, the block into a mechanism; shrink the prose by that  
much.

**Read the convention file first, whole.** Take the repository's  
convention as the file the owner names — `TBD-ROOTS.md` at the root by  
default. It names the roles, the trunk, the iteration file and where  
it lies, and the project's answers. Keep it the only statement and the  
only load path — no pointer file, no second copy.

**Stop on a repository that carries the marker with no readable  
convention file.** Say what is missing; produce no commit.

**Where the repository has no convention, load it explicitly on the  
first turn of a chat.** A refused `init` keeps it outside; a  
convention at the root is not injected — read it deliberately. Warn  
before any work; say where it comes from. Unloaded, the repository is  
unidentifiable and no commit is produced.

**Determine the roles first.** Read the request, decide the roles,  
work each in turn. Take a role as the nature of the knowledge, never  
the file, the country or the phase. Role: who speaks. Branch: where  
the work sits. No role — the commit is not understood. A parentless  
commit is the exception: `init` (no role, no iteration) or a numbered  
ordinary root as the project answered.

**Take no answer as an editorial act.** Never name, offer or schedule  
a rebase, a fold or a re-cut.

**Quote state from a command run in this turn.** Ahead and behind,  
counts, hashes, digests. A remembered number is not state; a claim  
without the command is a defect.

**Producing a commit**

1. Pick the role from its meaning.
2. Write `{N}|{role}: {subject}` — never omit or invent N; the literal  
   `N` belongs to `wip-` alone; keep the line ≤50 characters, aim  
   17–39.
3. Check the subject against every subject in the edition; a clash  
   stops the work.
4. Write the body as rule 1 — why the commit exists; open on the  
   value, never a definition or a count. **A message is UTF-8 end to  
   end**: take it from the object (`git cat-file -p`), never from a  
   console pipe — the pipes follow  
   [`generate the todo`](<TBD-ROOTS - generate the todo.md>),  
   *Encoding*; a body captured through the machine's code page comes  
   back with box characters — the defect. A `fixup!` body opens on its  
   heading. A correction names where its rule stands: the rule written  
   first into the convention, the correction pointing at its section.  
   No conflict markers in a commit. Each commit its own time, seconds  
   apart; a block replayed without its `break`s lands one timestamp  
   and the generator refuses it. **Every rewrite — an amend, a rebase,  
   a re-cut — stamps both dates with its time**: `--date=now`; a kept  
   author date through a rewrite is the defect. Test: after the  
   rewrite, `%aI` and `%cI` are equal and stand at the rewrite's time;  
   an unequal pair, or the old `%aI`, is the defect, repaired before  
   the push.
5. Fix a commit inside a `PR/` branch: the branch's own — amend it  
   (`git rebase -i` with `edit`, or reset and re-commit with the  
   author date) and force-push; a `fixup!` on it is a defect. The  
   `fixup!` form belongs to the todo stage; it may ride when its  
   target is not the branch's own — rule 5.
6. Find the target before writing the `fixup!` — rule 5. Read the  
   subject back: `git log -1 --format='%s' <sha>`; the `fixup!` line  
   reproduces it byte for byte. No target: not a `fixup!` — stop and  
   report. A sentence already in the tree is corrected where it was  
   written: find that commit and `fixup!` it; editing in place is a  
   rewrite. Cookbook: `git show <sha>:<path>`; the corrected lines  
   from the branch; blob `git hash-object -w --stdin`; throw-away  
   index `GIT_INDEX_FILE`; `git read-tree <sha>`;  
   `git update-index --add --cacheinfo 100644,<blob>,<path>`;  
   `git write-tree`; `git commit-tree <tree> -p <sha> -F <file>` with  
   subject `fixup! <subject>`. Check `git show --stat <new>` carries  
   that file and nothing else; name the hash in the plan after the  
   pick. No worktree, no detached checkout.

**Apply the subject form to picked commits.** A merge carries the  
standard text and takes its iteration from the commit that numbers it;  
`fixup!`/`squash!`/`amend!` name their target, are unnumbered, and are  
no entry of their own; none of the three corrects a commit its own  
branch introduced.

**Never put a fixup on a fixup** — all three forms, in the commit and  
in the plan. No chain; stop and name it. Re-amend or re-aim a wrong  
correction, never stack it. Correct a merge's message like any other:  
`fixup -C` on the line after its `merge`.

**Script only `fixup` of the three**: it keeps the message byte for  
byte. `squash` and `amend` open an editor; non-interactively `squash`  
commits the raw combination and `amend` replaces the message. Carry in  
an `amend!`: the original subject after the prefix, blank, the new  
subject on one line, blank, the new body.

**Treat `--amend` as committing the whole index** — check  
`git diff --cached --name-status`. A conflict does not preserve the  
message: `GIT_EDITOR=true` writes the `#` scaffolding into the body;  
use `git commit --amend -C <original>` and compare the bodies.

**Carry each commit's own `## Commit logs` block.** A `fixup!` in the  
reference; on a `PR/` branch an amend; run the refresh at each stop. A  
project without the log carries no block.

**Never rebase over uncommitted work** — `--autostash` does not save  
it. Check `git status` before any rebase or switch; copy the file  
aside or leave it.

**Treat the rebase-todo file as a script, enterable at any point.**  
`reset <label>` puts `HEAD` where the block acts; the live todo is a  
slice; restoring the passed part replays from the root;  
`git rebase --continue` runs once per break.

**`--continue` refuses while anything tracked is unstaged** (it says  
conflicts). A plan written into the tree stops the run until committed  
— the block's amend line is for that.

**Drive the run by `--continue` between breaks, by nothing else.** No  
hand chain: `commit-tree`, `update-ref refs/rewritten/*`,  
`reset --hard`, `checkout --detach`, `GIT_SEQUENCE_EDITOR`, `clean`,  
`stash`. Ask: *where does `HEAD` have to be next?* — git answers from  
the plan; computed by hand, the run has left the method.

**Write the live todo once per replay**, at the owner's ask; after  
that it belongs to git. Treat the tracked `last-git-rebase-todo` as  
the plan, committed at the stop that owns it.

**Start a replay at a break and end it at that same break**: the head  
and `msgnum` return to it. A `fixup!` at the tip is the proof the  
replay did not happen.

**Resolve a stop's conflict by the plan's order.** A folded correction  
meeting a dead path: keep the correction's version  
(`git checkout --theirs`, modify/delete). A path re-added later: keep  
the earlier version (`git checkout --ours`, add/add). Deleting loses  
the correction.

**Two silent stops**: `git reset` on an unmerged path leaves it  
untracked (use `git rm`); a killed step leaves `.git/index.lock`.

**`.git` is not UTF-8**: edit byte for byte — a copy first, a Latin-1  
round trip.

**The branch update line**, after the break:

```
exec git symbolic-ref -q HEAD >/dev/null || git update-ref refs/heads/<trunk>
```

The generator writes it.

**Cut a step by what it carries, not by what it touches**: cut only  
the line whose paths are all dead; a mixed line keeps its live paths  
and deletes the dead.

**A `--continue` failing at the same `msgnum` is not progress.** Read  
its output; call nothing again until what it named is settled.

**Read a block's name from the plan in block order**, skipping blocks  
whose `HEAD` did not move. Check the multiset: the non-merge commits  
and the plan's blocks hold the same names, no duplicate.

**A `fixup!` names the commit by the name it wears now** — the plan's  
`# …` line, not the original. An old name folds into nothing.

**Hold a stop one second before amending.** The rename keeps `%aI` and  
sets `GIT_COMMITTER_DATE=%cI`; the next block's `--date=now` orders  
the committer dates; two blocks in one second can swap.

**A block's shape: `generate the todo` owns it.**

**References go through labels, never hashes.** A hash in the file is  
a source of content, not a later reference.

**A root is `reset`, never `pick`**; a `fixup` after it yields a new  
root — `init`'s assembly.

**A change to `init` on an editorial rebase closes every open PR.**

**A correction to an earlier commit rides as a `fixup!` and is folded  
by the replay, never left standing** — the generator would write the  
line again.

**Describe the plan as the tree corrected**: the passed part carries  
the current-content hashes, the old `fixup` lines gone, the original  
order, labels re-placed.

**The file's own change rides in a `fixup! init` at the stop**, never  
in the working tree.

**The root line and the passed part are one generation behind by  
construction**; nothing downstream reads them.

**The generator orders by iteration, role, committer date.** Headers  
carry the reminder until the goal is put back by hand; a header still  
carrying it is unfinished; the header is kept up to  
`# Note for supporters:`; a duplicate committer second is skipped and  
named.

**Close an iteration**: merge or rebase the `PR/` branch into the  
trunk, rebuild the tree at the second rebase, hold the previous tree  
by its tags.
