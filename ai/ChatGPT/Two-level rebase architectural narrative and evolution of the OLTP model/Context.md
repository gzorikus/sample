# Context for Analyzing the `gzorikus/sample` Repository

## Important Perspective

Do not analyze this repository as a typical .NET sample project, utility library, framework, or collection of EF Core extensions.

The repository should be viewed as a combination of:

1. An architectural research project.
2. A proposed methodology for evolving large OLTP systems.
3. A Git-based knowledge management system.
4. A demonstration of a specific development process.

The source code is only one part of the repository's design.

The Git history, branch structure, commit messages, rebasing workflow, and supporting scripts are equally important architectural artifacts.

---

# Core Technical Theme

The repository explores alternative ways to design long-lived OLTP systems.

The author investigates questions such as:

* What is the true center of an OLTP architecture?
* Should transactions be primary abstractions?
* What role should repositories play?
* How should use cases be organized?
* How can inheritance be replaced by composition?
* How can EF Core become an implementation detail rather than an architectural center?
* How can infrastructure code be minimized without sacrificing maintainability?

The repository contains multiple branches representing different stages of this exploration.

Examples include:

* transaction-centric modeling;
* use-case segregation;
* repository minimization;
* composition-based dependency injection;
* state ownership concepts;
* metadata-driven infrastructure;
* EF Core integration strategies.

---

# Branches Are Architectural Experiments

Do not interpret branches as traditional feature branches.

Each branch represents an architectural hypothesis or experiment.

Examples:

* What if transactions become the primary OLTP abstraction?
* What if repositories are treated only as persistence infrastructure?
* What if use cases are separated by record types?
* What if inheritance is eliminated from repository design?
* What if EF Core only handles specifications while infrastructure is generated automatically?

The sequence of branches forms a narrative of architectural evolution.

Understanding the repository requires studying that sequence.

---

# Git Is Used as a Knowledge System

The repository uses Git for more than version control.

The goal is not only to preserve:

* code history;

but also:

* architectural reasoning;
* conceptual dependencies;
* evolution of understanding.

The repository attempts to version knowledge, not merely implementation.

---

# Two Different Histories Exist

## Research History

Created by developers during exploration.

Contains:

* fixups;
* experiments;
* temporary implementations;
* dead ends;
* debugging commits.

This history captures how understanding was discovered.

---

## Knowledge History

Created through rebasing.

Contains architectural conclusions.

Instead of commits such as:

* fix bug;
* cleanup;
* rename;

the preferred form becomes:

* Transaction is the heart of OLTP.
* Repository is infrastructure.
* Use cases must be segregated.

A commit represents a conclusion rather than a file modification.

---

# Two-Stage Rebase Model

The repository's methodology relies on two different rebasing stages.

---

## Stage 1 — Developer Rebase

Performed by individual developers.

Purpose:

Transform research history into knowledge history.

Example:

Before:

* fixup
* fixup
* experiment
* debug

After:

* architectural conclusion
* architectural conclusion
* architectural conclusion

The result is a branch that communicates what was learned.

---

## Stage 2 — Editorial Rebase

Performed by the team lead/editor.

This stage is described in the repository README and supported by dedicated scripts in the `scripts` directory.

Purpose:

Not cleanup.

Not squashing.

Not beautification.

Purpose:

Topological placement of knowledge.

New conclusions are moved into the existing narrative structure according to conceptual dependencies.

The editor determines where a new idea logically belongs within the evolving architecture.

---

# Repository History Is a Tree, Not a Timeline

Do not model repository history as:

A → B → C → D

Instead model it as a knowledge tree.

Example:

Transaction Boundary
├── Ownership
│   └── Metadata Sorting
└── Transaction Batching

Each node represents an architectural idea.

Each relationship represents a dependency between ideas.

---

# Discovery Order vs Explanation Order

The repository distinguishes two fundamentally different orders.

## Discovery Order

How developers actually arrived at a solution.

Example:

Ownership
→ Sorting
→ Transactions

---

## Explanation Order

How the architecture should be understood.

Example:

Transactions
→ Ownership
→ Sorting

The editorial rebase transforms discovery order into explanation order.

The final history preserves explanation order.

---

# Main Branch Philosophy

The main branch should not be viewed as a traditional integration branch.

Main represents the current canonical knowledge model of the project.

Commit placement is determined by conceptual relationships rather than creation time.

The history may be rewritten to preserve architectural coherence.

---

# Releases and Tags

Version tags serve as snapshots of the evolving knowledge tree.

Examples:

* v1.0
* v1.1
* v1.2

Each tag captures the state of both:

* the implementation;
* the architectural narrative.

Research branches can remain archived separately, preserving the original discovery process.

Thus:

* research history is preserved;
* canonical history remains coherent.

---

# Team Roles

Developers act primarily as researchers.

They explore solutions, create experiments, and document conclusions.

The team lead acts primarily as an editor.

Responsibilities include:

* maintaining narrative consistency;
* placing new ideas into the correct conceptual location;
* performing editorial rebases;
* preserving the integrity of the knowledge tree.

The team lead's role is closer to a scientific editor than a traditional merge manager.

---

# Tree Growth Metaphor

The repository is best understood as a growing tree.

Over time:

* roots become deeper;
* concepts become more refined;
* new branches emerge;
* existing branches may be repositioned;
* the visible trunk is regularly pruned and reorganized.

The project evolves by expanding its conceptual root system while continuously refining the structure of its narrative.

This metaphor is central to understanding the repository's development model.

---

# Guidance for Future Analysis

When analyzing this repository:

1. Study the README carefully.
2. Examine the branch sequence.
3. Analyze commit messages as architectural statements.
4. Inspect the scripts supporting rebasing and narrative maintenance.
5. Treat Git history as an architectural artifact.
6. Interpret branch evolution as a research process.
7. Evaluate code changes within the context of the evolving knowledge tree.

Avoid analyzing the repository solely as source code.

A significant portion of the architecture is expressed through the structure and evolution of the Git history itself.
