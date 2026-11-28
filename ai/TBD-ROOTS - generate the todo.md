## When the todo is generated

**Generate it. Never hand-edit. Walk it. Never re-plan it by hand.**

**This is the contract.** Generate exactly this. Where a  
[ready-made script](../scripts/CommitsToLastGitRebaseTodo.ps1)  
differs, the contract wins; report the difference in the  
implementation's header. Never read the todo's earlier blocks as a  
template; where they disagree, this text wins.

**Route first.** Take a todo as the area's route — the order a change  
in it is made — with today's items on it. Name in an item the step the  
route does not cover; correct the route in the commit that adds it.

**Input.**

```
git log --date-order --reverse --format='%D|%ct|%h|%p|%s' --decorate-refs="refs/heads/*"
```

Five fields at `|`: branch decor, committer seconds, short hash,  
space-separated parents, subject.

**Subject.** `^(\S*?)\|(.*?):` gives the iteration and role tokens;  
take the token's first word as the role. `N` is the wip iteration:  
number it like any other, sorted after every numeric iteration.

**Label.** Numbered commit: the subject with the role word replaced by  
its lowercase name, every `\W` → `-`, lowercased. `\W` counts non-word  
characters; a non-BMP emoji gives two hyphens, Cyrillic letters stay  
word characters. A label is a ref name, not the text. Unnumbered  
commit — a root among them: the subject as it stands, same  
substitution. A root with an iteration token is numbered and takes the  
numbered rule.

**Role index** is the role's position in the convention's array. The  
wip iteration carries none. `7` is the reference's choice, assuming at  
most seven roles. A role token outside the array stops the run, naming  
the subject; the ready-made script silently takes the last entry —  
report the difference, never copy.

**Treat encoding as part of the contract.** Git's output is UTF-8;  
decode UTF-8. Force Unicode at every pipe: the console's output  
encoding, the file's encoding, the git config for log output. A pipe  
that cannot be forced is a stop. The label is the one ASCII place;  
subjects, bodies, comments and the todo's bytes keep every character.  
Read the role array from the convention, never type it. How a taken  
script differs is the tool law's business.

**Classes**, first match wins:

1. **root** — no parent:  
   `reset <hash> # always reset onto the root tip first`. A root with  
   no iteration token is iteration `-1`.
2. **merge** — two or more parents: `merge -C <hash>` followed by the  
   label of every parent after the first, each referenced. Its own  
   label is the subject unchanged; an iteration token in its subject  
   still numbers it; without one it joins the unnumbered set of its  
   parents.
3. **`fixup|squash|amend!`** — targets the commit the rest names: a  
   new line, appended to the end of that target's command text:  
   `fixup <hash>`, `squash <hash>` or `fixup -C <hash>`, plus  
   ` # <first non-empty body line>` without its terminator, never a  
   stray `\r`. The comment sits on the line just added, or on the  
   target's own command line when this is its first. It carries that  
   commit's parent and unnumbered set. When its line holds a branch  
   decor, append the branch name to the head of the nearest ancestor  
   that has a command.
4. **ordinary** — `pick <hash>`. No iteration token or no role token:  
   a hard error.

**Number unnumbered commits by a descendant.** Each command carries  
the set of unnumbered ancestors it will number — inherited from its  
parents, plus itself when it is a merge with no token. A numbered  
commit takes the set and gives every member its iteration and role  
index. Check nothing at the end: a commit nobody numbers stays  
unnumbered, sorts after the numbered ones, prints no header. Narrow  
the error: a record that produces no command — a `fixup` line —  
carries no set. A `fixup` consumes the set it rides on.

**Duplicate committer time.** Key blocks by the committer second. A  
block whose second was used is that block's duplicate. Drop neither:  
comment both blocks' command lines out line by line — the  
`pick`/`merge`/`reset` and every attached `fixup`/`squash` line —  
while comments and `label` stay live. Follow the later one with a  
`break`; name the duplicated second in a footer; each keeps its  
`label`. Report the error and continue.

**Order.** Numbered iterations first, then iteration, then role index,  
then committer time. Not chronology: the input, never the output.

**Continuity.** Walking the sorted order, resolve each commit's parent  
through any `fixup` records to the nearest ancestor that produces a  
command. Where that ancestor is not the commit emitted just before it,  
prefix the command with `reset <its label>`, referenced.

**Emit.** Read the todo as UTF-8. Write in LF; normalize a CRLF file  
in the same act and report it. A head and a generated body. The body  
announces itself with a header, so find the cut by content, never by  
counting blank lines. The head is everything above the first line of  
three forms: `# <root subject>` as it stands, `# <Trunk>` with the  
trunk's first letter capitalised, or `# Startup Iteration <N>.`  
followed by anything. Drop that line and everything below; keep the  
newline ending the line above; no such line — the whole file is the  
head. The three forms are fixed; the goal after the dot is the  
human's.

**Whether a head exists is the iteration-opening answer.** Kept in the  
repository: a document, its head preserved byte for byte, never  
regenerated, never a rule. A kept head belongs to the script it was  
copied with; write the script or keep the todo elsewhere — the head is  
what the implementation needs, or none. Such a head never carries a  
line shaped like the three forms. Outside the tree: a transient plan,  
no head, replaced whole. No answer: treat as a document, keep the  
head, say the mode was assumed.

**Body.** Walk the sorted order and append, per block: its  
`# <subject>` line, its git command, one line per attached  
`fixup`/`squash`/`fixup -C` carrying that commit's first non-empty  
body line as a comment, then these four lines in this order:

```
exec git commit --amend --no-edit --date=now --allow-empty
exec git config --get alias.interactiverebasebeforebreak && git interactiverebasebeforebreak || true
break
# don't forget to amend now with updated stats in README.md
```

**Read all four as the contract.** `--date=now` keeps the committer  
dates in the file's order; the `break` makes the run stoppable. When  
the block carries a head — its own or inherited — then:

```
exec git symbolic-ref -q HEAD >/dev/null || git update-ref refs/heads/<branch>
```

A block opens a header when its iteration differs from the previous  
block's, or when its head is the trunk: `# <root subject>`,  
`# <Trunk>`, `# Startup Iteration <N>.` (N for wip). The root has no  
iteration header. Drop the goal after the dot; never carry it over.

**Treat the layout as a fixed point; the reading is not.** Separate  
blocks by one blank line; a second run produces the same bytes — that  
tests the writer, never the reading. Diff every run against the  
artefact the repository keeps —  
[the reference script](../scripts/CommitsToLastGitRebaseTodo.ps1) over  
the same commit, or the plan in the tree — and walk the difference  
line by line: a difference this contract does not name is the  
implementation's defect; the run stops before writing. A first run has  
no artefact: it produces the fixture over a named commit; the next run  
is held to it. Acceptance has a second half: every declared branch is  
walked; a class in no history is named as not walked.

**A label is referenced when something names it** — a continuity  
`reset <label>` or a `merge -C … <label>`. Find references over the  
whole emission before writing; print a label at most once, on its own  
block.

**Make failures loud.** A numbered commit with no role, a role outside  
the array, a `fixup!` whose target has no command, a `fixup!` whose  
target is itself a `fixup!`, a `fixup` line still carrying an  
unnumbered set: each stops the run with the subject named. A duplicate  
committer time is the one that continues.

**Pitfalls.** A body line may carry a trailing `\r`: strip it. An  
unnumbered ancestor is numbered by the first numbered descendant in  
parents-first order. An iteration token that is neither digits nor `N`  
stops the run. A head that instructs how to cut is valid for the  
script beside it, not for this contract.
