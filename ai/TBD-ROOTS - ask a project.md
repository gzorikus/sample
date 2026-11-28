## When a project is taken up

**Ask; assume nothing. Turn every answer into a mechanism, not a  
note.**

**Read the repository before the first question.** Where it carries a  
convention — `TBD-ROOTS.md` or the file the owner names — read the  
blocks it answers, never ask them again, and say what it read. Put  
only unanswered blocks to the owner.

**Ask the language first — where a questionnaire runs.** Ask, in the  
owner's language, which language for the session; conduct everything  
in it. Give this question even to a complete convention.

**Never answer for the owner.** Quote the answer. `init` or an  
ordinary first commit is question 4's answer, never a default.

**Ask about the project, never about the run's mechanism.** A path, an  
interval, a format, a name the run invented: decide it, record it in  
the map, and say so.

**Rules of asking.** Ask one question at a time; a block may be one.  
Number questions locally to a block. Treat a file as a draft until the  
questionnaire is done: mark what is unanswered; take the marker off in  
the act that closes the questionnaire. Defer a question whose consumer  
does not exist yet.

**Orientation**

1. **Purpose**, one or two sentences. *(No default.)* Name it  
   *Purpose* in the convention.
2. **Public or private?** *(No default.)* It decides the trunk's name  
   (3), the licence (4), and whether to reuse the reference's  
   `LICENSE`.
3. **The trunk's name.** *(Default: `main` private; `public` is the  
   reference's choice.)* Treat the repository's main-branch setting as  
   authoritative.
4. **The licence.** *(Private: none. Public: ask which, whose  
   copyright; never ship the reference's file.)*
5. **Is the copyright line in source files the same as the  
   licence's?** *(Default: yes.)*

**Roles**

1. **Which emoji represent the roles, in canonical order?** *(No  
   default.)*
2. **Each role's meaning in one phrase, what it leaves and where.**  
   *(No default; keep a hand-over's index at its folder's root; record  
   a hand-over that could not be made where it failed.)*
3. **Do the roles have lowercase names to match, index for index?**  
   *(Default: derive and confirm.)*
4. **What must not influence role choice?** *(Default: nothing; take a  
   role as the nature of the knowledge.)*

**The initial scheme — the moment the roles are settled**

Write the convention down to its `## Roles` block here and place the  
layout in the same act — one folder per role, its index at the  
folder's root, the hand-over between neighbours — before iteration  
opening and the README.

1. **Does one role's work depend on another's, in which order?** *(No  
   default.)* Take the answer as each role's hand-over: what it  
   leaves, where, who reads next — role by role, canonical order.
2. **What is the tree's root, which folders sit at it?** *(No default;  
   derived and confirmed.)* Create a folder only where a role leaves  
   something in it. Keep what the project brings from outside in the  
   folder of whatever it serves, never at the root.

**Never read a role as a person, chat, session or automation.** Read  
it as the nature of the knowledge a commit carries. Separate roles by  
nothing but a commit and a place: one role writes one place; two roles  
never share a file or folder; the hand-over is what the previous role  
left. No role, no folder. Never stop on «The role is missing»: the  
same run writes that commit. One commit with two roles has broken the  
boundary. Role names in this text are one project's answers.

**Keep a pilot a vertical slice** — the thinnest thing showing the  
domain working. Let the domain role write models and use cases only,  
with no third-party dependency, checked mechanically. Let the stack  
arrive through the façade on demand; keep adapters on the periphery.  
Let the first iteration deliver understanding, not a running program.  
Stand a pilot on a checked fact.

3. **What is the tree's first commit, what does it carry?** *(Default:  
   the convention file, its `## Roles` block and the scheme, with what  
   the roles' indexes need.)* Open the trunk on it; never retro-fit it  
   later; one `init`, every branch descends from it. Take a third  
   answer as no fault: **no common files inside the tree at all** — no  
   convention, no scripts, no README, no todo; keep them outside. That  
   is the refusal of `init`: the convention goes to the owner to  
   place, all three refused together; no todo, no generator, no alias;  
   void the questions that place any of it; the tree takes the commit  
   discipline alone. Do not ask what `init` carries here; a folder  
   plan is not an answer. Where `init` is refused the convention is  
   not in the tree: load it explicitly before any work, first time in  
   a chat.
4. **Is that first commit ordinary, or `init`?** *(Ask only where 3  
   took `init`.)* Ordinary: numbered and role-carrying —  
   `0|{role}: {subject}` — the rules artefact and iteration 0's  
   opening. `init`: no role, no iteration, subject `init`; the  
   numbered opening stands beneath it as its child. The number is  
   consumed: the generator reads `{n}|{role}:`; a root carrying it  
   gets an iteration header, one without falls to `$initIteration`  
   (−1). Ask whether `init` may be rewritten. *(Default: yes.)* A  
   rewriting `init` takes a `fixup! init` and folds into a new root at  
   every edition; a change to it closes every open `PR/`. A fixed  
   `init` never moves: the plan lands above it, no `PR/` closes, and  
   the renderer shows editions.
5. **What names the folders, what names a version of their content?**  
   *(No default.)* Name a folder for what its role works on; let the  
   answer tell a `PR/` branch's tree from the trunk's.
6. **How does a folder state itself, how does an empty folder stay?**  
   *(Default: one file at the folder's root, never a placeholder.)*  
   Take the file's name from the project's answer. State a folder in  
   exactly one file — the same file that hands over. Never use  
   `.gitkeep` or its kind. Put a rule the folder owns in that folder's  
   file. Let a name only a mask mentions have no folder. Where folder  
   and convention read against each other, let the folder's own word  
   win.
7. **What is the route for each area — the order a change in it is  
   made?** *(Default: write it, numbered, in the entry document.)* Add  
   a step a run had to invent in the same commit.

**Iteration opening**

1. **How must an iteration begin?** *(The todo file, at the project's  
   path — task and number fixed before work.)* The number rides in  
   subjects; the header derives; the goal is written by hand:  
   `# Startup Iteration 0. {goal}` with commits `0|{role}: {subject}`.  
   When: the act that creates the repository — the file takes `init`  
   and the first iteration's header, goal only. The `pick`s come with  
   the first `PR/`: a `fixup!` onto `init` plus one `pick` per commit.  
   The goal lives in the file, never in a body; a first number is  
   never taken from how much work exists. Where `init` is taken, the  
   opening commit is never it; `init` lays the repository down, stands  
   in no iteration, creates the root README.
2. **What is the todo file called, is it kept in the tree?** *(Not  
   asked where `init` was refused. Default `last-git-rebase-todo`; the  
   path is its own question. No single root commit — keep it  
   outside.)* Kept: a document, its head preserved byte for byte,  
   checked against the file; outside: a transient plan, no head. Where  
   the script lives is asked too: in the repository only by the  
   owner's agreement, otherwise in the harness's space.
3. **May this text propose switching iterations?** *(Default: yes,  
   when the goal is objectively closed.)*

**The PR format — asked when the first `PR/` is cut**

1. **Do the requests wear labels?** *(Default: no — the law adds none;  
   a project without labels answers nothing. Yes: one per verb, its  
   emoji and word, created the first time the verb appears, read from  
   the role statement, reused after.)*
2. **What do the editions' tags look like?** *(Default: flat,  
   unannotated, `{branch}-{iteration}`, one per branch per iteration,  
   only the trunk's tag moves. Offer this default; take the answer as  
   final. Do not study existing tags.)*

**The README**

*(Not asked where `init` was refused.)*

1. **Does the root `README.md` change after `init`?** *(Default: yes —  
   a living file the two log tools keep.)* No: the octopus shape, many  
   roots side by side; no file growing on every break; both tools and  
   the graph refused; do not ask questions 2, 3, 5–7; the alias stays  
   question 4's.
2. **Is a commit's body written into `## Commit logs` by script?**  
   *(Ask after 1 is yes; default yes.)* Writes blocks only for counted  
   commits: no block for a merge, a fixup or a commit touching nothing  
   in the mask. No: the section is not maintained; question 3 decides.
3. **If the log is refused, what happens to the two tools?** *(No  
   forced default.)* Out — the default for a tree with no log and no  
   entry-point block. Kept — for a project bringing the log back:  
   implemented and invoked before every `break`, writing nothing while  
   the section is absent.
4. **Is the alias `interactiverebasebeforebreak` installed?** *(No  
   default; asked wherever `init` was taken.)* Its body is the refresh  
   alone. Keep `commit.cleanup = verbatim` and `include.path` whatever  
   the answer.
5. **Does every entry-point branch need its own block?** *(Asked after  
   1 is yes; default yes.)* The reference exempts the trunk and fails  
   on any other branch without a block.
6. **What does the table show — row unit and columns?** *(Reference: a  
   row is a commit, columns `Log`, `Examples`, the line count. No code  
   → no line count; no examples → drop the column.)*
7. **The block's head: keep `Pros:`/`Cons:`, or write your own?**  
   *(Default: keep.)*
8. **Which folders does the line-count metric leave out?** *(Default:  
   `examples` alone, shown rather than asked; only excludes are asked;  
   `obj`/`bin` out but not listed; a listed name with no folder is not  
   a fault.)*
9. **Is the wrap taken?** *(Default: yes; where the log is refused,  
   off by default and no run wraps. Refused alone: rule 17 of  
   `wrap a text` never fires; `wrap-check` is `not applicable`.)*

**The graph in the report**

Read this block as this text's knowledge, not a project's convention:  
the project answers only whether it wants the graph and where it  
stands. Where the common file does not change, never stand the graph  
in it. Learn the drawing from the reference's graphs and a tree's own,  
in progression. Take the form, traps, checks, algorithm and fragments  
from the `graph-cookbook` artefact.
