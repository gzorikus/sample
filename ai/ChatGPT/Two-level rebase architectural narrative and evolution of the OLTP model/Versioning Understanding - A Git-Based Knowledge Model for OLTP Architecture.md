# Understanding the Purpose of This Repository

## What Is This Repository?

At first glance, this repository may look like a collection of .NET  
libraries focused on EF Core, OLTP applications, dependency  
injection, and reflection-based infrastructure.

That interpretation is only partially correct.

The repository should be viewed as an exploration of how large OLTP  
systems can be designed, evolved, and documented over time. The  
codebase is the result of that exploration, but the repository also  
contains the process that produced the code.

To understand the project, it is not enough to read the current  
source code. You must also study:

* branches;
* commit history;
* commit messages;
* development scripts;
* rebasing workflow;
* architectural evolution documented through Git itself.

The repository treats Git as a knowledge management system, not  
merely a version control system.

---

# Technical Goal

The technical side of the project explores several architectural  
questions:

* What is the true center of an OLTP system?
* Should transactions be the primary abstraction instead of  
repositories?
* How should use cases be organized?
* How can inheritance be replaced by composition?
* How can EF Core become an implementation detail rather than  
a central architectural component?
* How can infrastructure code be minimized while preserving  
explicit business intent?

The resulting architecture tends to favor:

* transaction-oriented modeling;
* use-case segregation;
* composition over inheritance;
* explicit ownership of state;
* metadata-driven infrastructure;
* reusable OLTP building blocks.

---

# Why the Branches Matter

The branches are not traditional feature branches.

Each branch represents an architectural experiment.

Examples include questions such as:

* What happens if transactions become the primary abstraction?
* What happens if repositories are treated as infrastructure only?
* What happens if use cases are separated by record types?
* What happens if dependency injection is built around composition  
rather than inheritance?

The branches form a sequence of investigations.

Reading them in order reveals the evolution of the underlying  
architectural model.

---

# Git History Is Part of the Architecture

Most projects use Git to answer:

> What changed?

This repository attempts to answer a different question:

> What was learned?

As a result, commit messages are treated as architectural conclusions  
rather than descriptions of file modifications.

Instead of:

* fixed bug;
* refactoring;
* cleanup;

the preferred form becomes:

* Transaction is the heart of OLTP.
* Repository is infrastructure.
* Use cases must be segregated.

A commit documents a conclusion.

---

# The Two-Stage Rebase Process

One of the most unusual aspects of the repository is its use of  
rebasing.

## Stage 1: Developer Rebase

Developers work normally.

Their branches may contain:

* fixups;
* experiments;
* temporary implementations;
* failed attempts;
* debugging commits.

Before integration, these commits are reorganized into a small set  
of meaningful architectural statements.

The goal is to transform:

research history

into

knowledge history.

---

## Stage 2: Editorial Rebase

This is the defining characteristic of the repository.

A team lead (or editor) performs an additional rebase using dedicated  
scripts provided in the repository.

The purpose is not cleanup.

The purpose is topological placement.

New architectural conclusions are moved to the location in the existing  
narrative where their dependencies naturally belong.

The result is that commit placement is determined by conceptual  
dependency rather than chronological order.

---

# Thinking About the Repository as a Tree

Do not think of the repository history as a timeline.

Think of it as a growing tree.

Each commit represents an idea.

Each parent-child relationship represents a dependency between ideas.

Over time:

* roots become deeper;
* concepts become more refined;
* new branches emerge;
* the visible trunk is regularly pruned and reorganized.

The repository evolves more like a knowledge graph than a  
traditional software history.

---

# Chronological Order vs Logical Order

The repository distinguishes between two different sequences.

## Discovery Order

How the team actually found a solution.

Example:

Ownership
→ Sorting
→ Transactions

## Explanation Order

How the solution should be understood.

Example:

Transactions
→ Ownership
→ Sorting

The editorial rebase converts discovery order into explanation order.

The final history preserves understanding rather than chronology.

---

# How to Read the Repository

A newcomer should not start with the latest code only.

A recommended approach is:

1. Read the [README](/README.md).
2. Study the branch names.
3. Follow the branch sequence chronologically.
4. Read commit messages as architectural statements.
5. Examine the scripts that support the rebasing workflow.
6. Analyze how concepts move and evolve between branches.
7. Finally, study the current implementation.

The code becomes significantly easier to understand once the  
architectural narrative is understood.

---

# What Makes This Repository Different

Most repositories version code.

This repository attempts to version understanding.

Most repositories preserve the history of implementation.

This repository attempts to preserve the history of architectural  
reasoning.

Most repositories treat Git as storage.

This repository treats Git as a medium for documenting and  
evolving a shared mental model of the system.

For that reason, the commit graph, branch structure, and  
rebasing workflow should be considered first-class architectural  
artifacts, just as important as the source code itself.

# To be continued

You can continue chatting in your LLM of choice by copying  
[Context.md](./Context.md).

Happy studying!
