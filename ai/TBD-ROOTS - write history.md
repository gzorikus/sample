## When history is written

**Write history as assertion, not as a log.**

1. **Write the body as the reason the commit exists.** Write the  
   prehistory: the state before, what forced the change, what was  
   rejected, what the diff cannot recover. Test: would any other  
   commit ever read this line? Never restate a path, a value, a name  
   another commit reads — it is that commit's subject matter. Never  
   describe the mechanics — describing the act repeats this file. An  
   empty body stops the work. Exempt merges — rule 3.
2. **Write a `fixup!` body as the reason too, and open it with a  
   heading.** One line, a blank line, then the description. The todo  
   generator takes the first non-blank body line and writes it onto  
   the `fixup` line as its comment. Make the heading say why, not  
   what.
3. **Give a merge the standard git text and no body.**
4. **Keep subjects unique within an edition.** A duplicate is a hard  
   stop. An act that recurs under the same words — a counter, a queue  
   number, a count, a hash, a label — is not knowledge: make it a  
   `fixup!` onto the commit it deepens. Ask before writing: will this  
   act recur under these words in the next branch?
5. **Target the fixed commit's subject** — never a line, a file, a  
   hash. Never target a numbered commit the same branch carries: amend  
   that commit and force-push. Keep one `fixup!` per essence; fold a  
   second correction of the same essence into the standing one; give a  
   different essence its own. **Deepen with a `fixup!` what the trunk  
   holds**: a branch that continues the roots' work opens no numbered  
   commit; number only knowledge the roots do not hold. The branch  
   carries the fixup as its single commit and amends it round after  
   round. **Find the target, never guess it**:  
   `git log --follow --name-status -M -- <path>`; take the newest `R…`  
   line, else the `A…` line. Skip a `fixup!` that moved it; never a  
   merge; never the newest touch or the group that wrote the bytes.  
   **Amend and force-push a `fixup!` landed under the wrong subject**,  
   its subject set to the target, its tree left as it stood. Write the  
   correction into the todo's comments: what it carries, the subject  
   it carried before, the block it folds into; a todo outside the  
   repository keeps the note outside. **Stop and say so where the  
   branch has no target — the file is created here or does not  
   exist**; the edit is not a `fixup!`. Check an executor's copy of  
   this rule against this file:  
   `git log --diff-filter=A --format='%s' -- <path>` without  
   `--follow` is wrong. **Allow one exception — a cycle the work is  
   built on**: a `fixup!` on the branch's own commit, only where the  
   repository documents that role's commits as deliberately cyclic, in  
   its own words; never infer it from the shape. A cycle the project  
   does not name: stop and ask, and amend until the answer lands.  
   **Never leave a change loose**: at a rebase break, amend it into  
   that break; on a `PR/` branch, land it as a `fixup!`; locally,  
   start from a `fixup!`. A commit that is not a `fixup!` states where  
   it embeds; the rebase never guesses it.
6. **Use three branch classes only: trunk, entry-point, `PR/`.** Treat  
   a `wip-` branch as an entry-point with a maturity marker. Give an  
   entry-point no name form: bare, unprefixed, long-lived. `PR/` marks  
   the other class: cut for one landing; never reuse its name unless  
   the repository says otherwise.
7. **Start a `PR/` branch from the trunk's latest head; keep it only  
   with its PR; birth it only on a request. Run these lines; stop on  
   the first that fails.**
   1. Run `git rev-parse <trunk>`.
   2. Run `git worktree add ../repo-<name> -b PR/<name> <trunk>` — the  
      folder begins with `repo`.
   3. Run `git merge-base --is-ancestor <trunk> HEAD` — exit 0;  
      anything else: stop.
   4. Never cut from another `PR/` or an entry-point. Birth an  
      entry-point at the second rebase, never before.
   5. Never read an empty-looking trunk as a reason to branch  
      elsewhere; recreate the branch, never move its base. **Cut only  
      on a request.** Cut the branch in the same act that opens its PR  
      — never alone, never to park a commit, never «just in case». A  
      second branch beside the one asked for is a defect. An object  
      that must travel travels inside the asked branch. No request, no  
      branch; never read the absence of a «no» as a «yes».
   6. **Read the change request as the request.** Open a `PR/` branch  
      for any request to change the project — including one that only  
      follows from earlier work — and keep the work it entails inside  
      that branch. **Before cutting, check for a branch that already  
      refines the same thing**: an open `PR/` whose request covers it,  
      or a run working it now. Found — stop and offer to delegate the  
      change to that run; a second branch for the same refinement is a  
      defect.
   7. Write the commit: number new knowledge; give a deepening the  
      target by rule 5 and `fixup! <its subject>`.
   8. Amend or force-push as  
      [`commit and replay`](<TBD-ROOTS - commit and replay.md>) says:  
      the author date is kept; the date test runs before the push.
   9. Run `git push -u origin <branch>`; later  
      `git push --force-with-lease origin <branch>`. Its author  
      creates the PR and keeps it.
   10. Open it in the same act as the branch, **as a draft**; make it  
       ready only when the branch is finished.
   11. Title: the subject the branch carries, spelled exactly —  
       `PR/<thing>` is `1|🔭: harvest <thing>`; a branch whose whole  
       content is one `fixup!` is titled by that fixup.
   12. Body: what it holds and why. Report the rebase when the trunk  
       moves.
   13. Take the PR's format from the project's answer — labels among  
       it: [`ask a project`](<TBD-ROOTS - ask a project.md>), *The PR  
       format*. The law adds no label and no tag.
   14. Keep it on the head. Rebase onto the current head whenever the  
       trunk advances; wait for no reminder. Resolve a conflict where  
       one side only rides along. Do not resolve a conflict where both  
       sides changed the meaning: stop, leave the conflict standing,  
       name the two sides, ask the owner. The rebase and the  
       force-push are the executor's; the merge is the owner's.
   15. Read the branch list against the open requests: name each  
       branch without one and each PR that breaks the project's  
       format. Mark a `PR/` branch with no open request, a branch  
       whose PR was closed, and a PR breaking the format as the  
       author's defect. Treat a branch that cannot be given a PR as  
       cut wrong. **Treat a name as the owner's, never the author's to  
       tidy**: rename nothing — branch, request, label, automation —  
       without a word that names the thing.
8. **Require a `wip-full-solution` branch beside every `wip-`  
   branch.**
9. **Merge only where the knowledge depends on several pieces.** Make  
   an octopus merge vertical compression when it is conflict-free.
10. **Let the trunk receive; assemble nothing on it.** Run  
    `git rev-list --max-parents=0 --all` — one hash: the rule applies;  
    several: read the last paragraph.
    1. Build on a branch cut from the trunk's head: one `PR/` per  
       request (rules 7, 18).
    2. Name it for what it carries — the batch and its date. Never  
       show its inner levels on the trunk.
    3. Hand it over as an open PR. The owner merges.
    4. Make the merge one merge and the stump: below it the roots and  
       the seam, above it the trunk.
    5. Make new knowledge above the stump a new numbered commit of its  
       own iteration, unique, its body naming its place among the  
       roots.
    6. A `fixup!` above the stump deepens a commit the roots hold; it  
       is a finished commit, re-cut at the editorial rebase.
    7. Count over history split at the stump: below roots and seam,  
       above trunk.
    8. Never build on the trunk. **No common root**: the rule does not  
       reach that tree; merge unrelated histories only with  
       `--allow-unrelated-histories`. Never read an empty-looking  
       trunk as empty: one `fixup!` above the stump fills it. Read the  
       roots by the scale of the received branch's name.
11. **Rebuild the whole tree at the second rebase; give each branch  
    one tag per iteration** — flat, unannotated,  
    `{branch}-{iteration}`. Never move tags, except the trunk's inside  
    an iteration. Treat the shape as a default:  
    [`ask a project`](<TBD-ROOTS - ask a project.md>), *Iteration  
    opening* 4 proposes it; the tools read the answer.
12. **Number every commit that reaches the tree  
    `{N}|{role}: {subject}`; set no upper bound.** A merge takes its  
    iteration from the commit that numbers it; a `fixup` takes no  
    number; local scaffolding never reaches the tree. `N` belongs to  
    `wip-` alone. The first iteration is `0`. Open an iteration, never  
    discover it: fix its number and goal before work starts.
13. **Place placeholders in advance.** A `fixup` introduces the  
    placeholder, a later commit the data. On a `PR/` branch the  
    placeholder rides in a clean commit, never in a `fixup!`;  
    force-push the branch.
14. **Never treat a placeholder as an invariant.** An unfilled  
    template is an omission.
15. **Rebuild the whole tree each iteration**: a fresh commit per  
    branch; the tag keeps the past addressable, not the branch.
16. **Keep all iterations in one todo file; read the current iteration  
    and its goal only there, in its comments.** **Bring the passed  
    part current at a replay.** 1. Restore the passed part; correct it  
    against the standing instructions and the needed hashes. 2. Walk  
    the breaks from the beginning; at each, make the changes and amend  
    them into that break. 3. Stop at the break the replay started  
    from. Never rebuild the plan from nothing; never rebase for its  
    own sake. **Let the plan carry itself.** The amend that writes the  
    todo delivers it: the `init` block gains `fixup <hash>` naming the  
    `fixup! init` made in the same act, between the block's `reset`  
    and its `exec`. Make it now — a hash from an earlier generation  
    names a dropped commit. Carry the plan without that line — the  
    fold applies the carrier's diff; the plan `init` ends with is the  
    clean one. Stand on the break's own commit, never the tip: the tip  
    carries a later generation and the fold conflicts. **Write the  
    plan — six things.** 1. A block keeps its own `reset` lines; never  
    open with `pick` where the original opened with `reset`. 2. A root  
    block is `reset <root hash>` and no `pick`; `reset` resolves a raw  
    hash. 3. Write a merge whose text changes without `-C`, the wanted  
    text after `#`. 4. Write in LF; a text-mode write turns every line  
    into CRLF. 5. The iteration header carries the goal; never commit  
    the generator's reminder — it is unfinished. 6. The tail is  
    `# Main`, one empty line, `reset init`, the merge with its two  
    `exec`s and its `break`, the note, and  
    `update-ref refs/heads/<trunk>` on the very next line. **Check  
    before reporting.** Count `pick`, `exec`, `break`, `label` against  
    the reference. Compare every passed commit's parent with the  
    `reset` above it: `git log -1 --format=%p`. Resolve every hash to  
    a commit. Declare every label before the `reset` or `merge` that  
    names it. Never pick a commit carrying a body of `#` lines: that  
    is `commit.cleanup = verbatim` keeping the editor's scaffolding.  
    **No scratch clone**: prove the fold with

    ```
    git merge-tree --write-tree --merge-base=<the carrier's parent> <init> <carrier>
    ```

    **Never abort a replay.** Correct a wrong plan with  
    `git rebase --edit-todo` at a break; a restart re-does what stands  
    and drops the labels in flight.
17. **Ask the todo file's path once; it need not be inside the  
    repository.** Default `last-git-rebase-todo` at the root. Where  
    `init` was refused there is no todo. Every rule here means that  
    path.
18. **Create a PR together with its branch**, never close and  
    re-create it; never reuse branch names; the request that names the  
    work opens it (rule 21). Tidy commits in the PR branch before the  
    trunk. End the work in an open PR, not in a branch. Let the owner  
    alone merge, close or rewrite.
19. **Measure a message, never eyeball it.** 1. Subject: one line, ≤50  
    characters including `{N}|{role}: `; aim at 17–39. 2. Body: the  
    measure, fill, verbatim set, tool and check are  
    [`wrap a text`](<TBD-ROOTS - wrap a text.md>) — 70 of text plus  
    two spaces, at most 72 of raw line. One text, one home: this rule  
    points.
20. **Never name in a commit what stands above it.** Name only what  
    stands below it: the commits it answers and the concepts the tree  
    holds when it lands. A later name or idea is the defect. Declare a  
    contract instead: where work will go, what a form will hold. One  
    exception: a place two parallel roots both change is reserved in  
    advance, empty. Check mechanically: resolve every subject the  
    tree's files quote against the position of the commit carrying the  
    file; a subject above it is the violation. Hold a lane picture to  
    the same measure.
21. **Push a branch and open its PR on the request that names the  
    work; leave the merge to the owner.** The commit and the PR are  
    the run's; never wait for permission to commit. A change left  
    uncommitted in the tree is a defect. The convention says when a PR  
    opens and which roles work in it, not whether the push may happen:  
    the push follows the commits in the same pass. A role merges  
    nothing; a role closes or rewrites no PR. Scratch branches are the  
    role's and carry no PR.
22. **Send a request no rule covers into the trunk.** 1. Make the  
    commits on the trunk's head; push into the trunk where that push  
    is authorised. 2. Ask: does the request itself belong in the  
    convention? 3. If it does, land the entry in the same push.
23. **Do nothing without the owner's word or a duty the convention  
    gives a role.** Ask about anything else before taking it, one  
    question at a time. Read the duty from the convention, never infer  
    it.
24. **Read the root as the project's truth.** Read `init` and every  
    standing `fixup!` on it before the first write. A standing fixup  
    is the latest generation of the skeleton, not work to do.
25. **Correct with a `fixup!` on exactly one commit.** Find the target  
    by the knowledge corrected, never by the file. No target, or two  
    or more: stop and ask. Add no section to a correction: a heading,  
    a new rule, or content its target never mentioned is new knowledge  
    — stop and ask. Keep a correction inside one role's folder.
26. **Give a `fixup` no subject, iteration or role of its own.** They  
    come from its target; `N|role: …` makes a new statement. 26a.  
    **Rename a statement with an `amend!`.** Aim it outside the branch  
    only on the owner's word, and only with no `PR/` branch open at  
    the rebase. A `fixup` still naming the old name while any `PR/`  
    stands is the defect.
27. **Treat deleting another's text as new knowledge.** Without the  
    owner's word the text stays.
28. **Write no new knowledge without his word.** Test: would the owner  
    learn from this commit something he did not say? New is any commit  
    that is not a `fixup!`/`amend!`/`squash!`. A correction to what  
    stands needs no word. A merge is never a run's.
29. **Write one rule, one line.** One imperative statement,  
    addressable by the slug of its heading; send the rationale to the  
    article.
30. **Keep one home per fact.** The convention, this text, or an  
    artefact; everything else points. Keep headings unique in their  
    file; renaming one updates every reference in the same landing.
31. **Name no harness in a rule.** Speak of the repository and the  
    method; a harness appears only in an answer and in the artefact  
    carrying it.
32. **Never delete a statement the owner calls an error — find its  
    content a home.** Find the one commit that owns that knowledge —  
    the same search a `fixup` target gets — and land it there as a  
    `fixup!`. No home, or knowledge he did not give: stop and ask.  
    Drop only what he calls wrong, by his rebase.
33. **Never imply a rebase by a landing.** Two rebases: the  
    developer's, inside its branch, on the executor's word, never  
    touching the trunk; the editorial pass, the owner's, which re-cuts  
    the tree. Landing is commits — `fixup!` onto their homes — and a  
    push. Treat the editorial rebase as an item of its own, started  
    only when the owner names it as that act; a paraphrase is not his  
    word — quote it by place or ask. Before: regenerate the plan from  
    the tree it will rebuild, one worktree, a clean tree, the return  
    commit recorded, every statement the landing must carry named.  
    After: search the tree for each statement; a landing that folded  
    them away is the failure.
34. **Read a convention line as the project's answer or a restatement  
    of this text.** Take the words carrying the rule and search this  
    text and the installed carrier. Found: a restatement, the method  
    owns it — record it, remove it only by the owner's word. Not  
    found: the project's knowledge, it stays. “This looks like the  
    method” is not a test.
35. **Write a rule on a failure, not a remark.** Record the pitfall or  
    the bug before the fix, in the `fixup!` that carries it — the  
    case, the boundary, the expected; then correct or add the  
    instruction where it is read, strictly imperative. A fix whose  
    body does not carry the record is the defect. The loop:  
    [`take feedback`](<TBD-ROOTS - take feedback.md>).
36. **Give every area a route: the order a change in it is made.**  
    Write it once in the entry document, numbered. A run follows it  
    and never re-derives it; add a step it had to invent in the same  
    commit.
37. **Read the convention as what must be; the history as what was.**  
    Act on the convention, never on the history. Where they disagree,  
    the history is right; the convention is stale — correct it as its  
    own commit, never argue it away.
38. **Write every rule as an order a run can execute without  
    deciding.** One bold sentence: what to do. Then the test, the  
    boundary, the defect. One order per sentence, one idea per line.  
    No reasons, no options, no «consider», no «you». Name the actor —  
    a run, the owner, the tool. If a rule needs a «because», the run  
    has not decided it: decide it or ask.
39. **Work each branch in its own worktree.** Put the trunk, every  
    entry point and every `PR/` branch in separate worktrees beside  
    the clone; a checkout never switches branches.
40. **Script every action.** Perform no act by hand: run the file the  
    map's `mechanisms` names, hash and all. `code` is the only  
    default: `assisted` and `manual` stand by the owner's word;  
    `manual` is a debt, never a home. The workspace runs without the  
    model: an order is a command, never a reading.
41. **Record every new word the owner speaks.** Take a word the text  
    does not carry and fix its meaning in the owner's own words; add  
    it to the vocabulary of its kind — a project's word to the  
    convention, a section of its own where none holds it; a method's  
    word to `deploy`'s *Words* — and land it as a `fixup!` on the  
    commit that owns that vocabulary. Expand the language only as far  
    as the work needs; never widen an old word to cover a new one,  
    never paraphrase it away. No recorded word, no work under it: stop  
    and ask.
42. **Merge fast-forward only; delete the branch and its worktrees in  
    the same act.** Land a `PR/` branch into the trunk as a  
    fast-forward; never write a merge commit for it. Take the owner's  
    word to merge as the word to delete: after the merge, delete the  
    branch — local and origin — and remove every worktree that carried  
    it; leave neither standing, reuse no name (rule 6). Ask for no  
    second word.
