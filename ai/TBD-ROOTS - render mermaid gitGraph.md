**Render the named range of the log as one `mermaid` `gitGraph` block,  
and put nothing else in the answer.** No range named — ask. Draw each  
commit of knowledge as one lane; treat a branch as only a tag on the  
lanes it touches. Never draw the workflow's branches as the skeleton.

**Take** the log range, its merges, and every commit's subject as the  
log states it.

**Write** one `mermaid` `gitGraph` block:

1. Open with `config`: `mainBranchName` = the trunk,  
   `parallelCommits: true`, `rotateCommitLabel: false`,  
   `showCommitLabel: false`.
2. Ride labels on the active line as `tag:"..."` and carry the  
   commit's own subject. Never a hash. Never invent a number.
3. `checkout` a branch, never a commit; name the trunk bare.
4. One block per group: its root, the branches cut inside it, their  
   children, then the group's `merge` right after the group's last  
   commit, before the next group opens.
5. Name a lane after the commit it carries:  
   `branch "<the subject of its newest commit>"`.
6. Issue merges from the bottom lane upwards: the lowest child closes  
   first and takes the earliest column.
7. Fill a lane's gap with `commit type:REVERSE`; the count is the  
   span's width minus the branch's real commits.
8. Order by commit date, topology breaking ties: one tick per commit,  
   groups contiguous, each floor's merge one tick past its own group.

**Never:**

- a hash in a label — every id goes stale silently;
- a commit id in `checkout` — it does not parse;
- a merge before the commits it joins — the fence draws in written  
  order;
- one `merge` for N parents where N > 2 — it is N−1 lines;
- a filler without `type:REVERSE` — it draws a commit that never  
  existed;
- a branch name read as a claim about the repository — inside the  
  fence it is a drawing device;
- a number or a role read into a subject;
- a shape the history does not carry.

**Prove it with four checks, in this order:**

- **Count** — make `commit` lines equal the subtree's non-merge  
  commits; make `merge` lines equal one per parent beyond the first.
- **Parse** — make the block parse under a mermaid parser; a static  
  pass confirms every `checkout` and `merge` target was declared by an  
  earlier `branch`, and every merge is preceded by the commits it  
  names.
- **Idempotency** — read the fence back, generate again: the same  
  bytes.
- **Render** — measure, never admire: render the fence, read the  
  edges' polylines, confirm no two cross except at a node and  
  neighbouring lanes keep their distance. A preview is not a  
  measurement.

**Record the renderer and its version.** Grade the render check  
`measured` | `reviewed` (the owner looked, with the date) |  
`unavailable`. Pass only `measured`; record the other two as what they  
are. No renderer — report the check as not performed.

**Accept with the fragments.** Each is a minimal history with  
artificial names (`c1`, `b1`, `m1`), the fence it must produce, and  
what breaks when drawn otherwise. Pass a renderer that reproduces  
them; fail the one that does not, where the fragment says.

**F1. A line.** `c1 → c2 → c3` on `main`. Three `commit` lines, no  
`branch`, no `merge`. *Breaks on:* a `branch` where no branch ever  
existed.

**F2. A fork and one merge.** `c1`; `b1` from `c1` carries `c2`;  
`main` carries `c3`; merge `m1(c3, c2)`.
```
gitGraph
    commit id: "c1"
    branch b1
    commit id: "c2"
    checkout main
    commit id: "c3"
    merge b1 id: "m1"
```
*Breaks on:* a `merge` before the trunk's own commit, and a `checkout`  
of a commit instead of a branch.

**F3. An octopus of three.** `c1`; `b1`→`c2`, `b2`→`c3`; merge  
`m1(c1, c2, c3)`. Two `merge` lines — N−1 for N parents — and the  
lanes named in the reverse of the parent list. *Breaks on:* one  
`merge` line for three parents, and lanes in the recorded order.

**F4. Nested spans.** `c1`; `b1`→`c2`; inside `b1`, `b2`→`c3`; merge  
`m2(c2, c3)`; then `m1(c1, m2)`. F2 twice, on two floors, issued  
bottom-up. *Breaks on:* merging top-down — the upper lane closes first  
and every curve below crosses it.

**F5. A gap in a lane.** `c1`; `b1`→`c2`; `main`→`c3 c4 c5`; merge  
`m1(c5, c2)`. The lane must reach the merge column with fewer commits  
than columns, so `commit type:REVERSE` fills the difference: span  
width minus real commits. *Breaks on:* `type:NORMAL` for a filler, and  
no filler at all.

**F6. A root at the foot.** `c1` parentless, then `c2 → c3`. `c1`  
first, everything above it. *Breaks on:* a `branch` before the first  
commit, and a second parentless commit.

**F7. Every drawn line carries its tag.** A line that draws something  
ends with `%% <TAG>`. `checkout` draws nothing and carries none.  
*Breaks on:* the tag on the first line of a lane only.

**F8. A line that is not in the history is not drawn.** Commenting a  
region out is a rebase device for a file two parallel roots both  
change; never a way to show a shape the history does not carry.  
*Breaks on:* drawing an intended shape.
