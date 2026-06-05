# Sample

Hey, it's Georgii here 👋

Meet an enterprise software developer faced a lot of mature systems  
successfully competing in the market. Sounds too good? 😏 Unless  
bringing numbers of greenfield projects you'd surely face consequences  
of that "success" 🙈 When it comes to **"what took you so long?"**.

Here is a half decade pursued and finally embodied solution to the  
problem of **degrading development performance**. It's tied up with  
canonical production ready approaches for typical enterprise software  
components. You'll find reasoning to your own framework ownerhip at a  
complexity level you decide.

<details>
<summary><strong>⚠️ WIP warning ⚠️</strong></summary>

There's lack of some modules and examples that are to be implemented  
in the near future. Hopefully the published work will hint you what to  
expect next and decide whether to go deeper. Here are some TODOs:

|||
|-|-|
| 1. Example per commit | <kbd>... covering implemented modules</kbd> |
| 2. Modular split table | <kbd>YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition.EFCore</kbd> |
| 3. Events with native broker client | <kbd>YourCompany.OLTP.StateOwnership.EventsProducing.RabbitMQ</kbd> |
| 4. The outbox | <kbd>YourCompany.OLTP.StateOwnership.EventsProducing.RabbitMQ.EFCore</kbd> |
| 5. Events processing | <kbd>YourCompany.OLTP.RecordsManagement.DI.RabbitMQ</kbd> |
| 6. Cross-cutting processing | <kbd>YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition.RabbitMQ</kbd> |
| 7. Auto-generated full fledged REST API | <kbd>YourCompany.OLTP.RecordsManagement.AspNet</kbd> |
| 8. To be continued | <kbd>... other reusable modules</kbd> |

</details>

## Goals

1. Comprehensive educational material for beginners (or your "Agents")  
focused on bringing "human-understandable" business value rather than  
~~the-faster-and-more-code-the-better~~.
2. Exhaustive open-source boilerplate covering all typical needs of  
enterprise development properly put together for easy adoption to your  
ownership at the entry point of choice.
3. Shift the industry and protect the mass of projects from mistakes  
leading 9 of 10 to unmaintainable state after 3 years developming  
in ~~Deadline-Driven-Development environment~~ (DDD 🤣🤣🤣)...  
no matter architectural choices of its time.
4. Bring personally gained experience in a form that is too complex to  
achieve while in rush on those 9 of 10.

## Metrics

### Measure-Object -Line

The most important one 🤔 As practice shows, the amount is the main  
performance killer 💀 (esp. by duplicating and glue-code bloating 💩).

The rule is **The less code doing the job, the more value it brings.**  
Each piece you find here is evaluated with this metric.

```powershell
Get-ChildItem -Exclude examples |
    Get-ChildItem -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -match '^(?:(?!examples\\).)*src\\YourCompany(?:(?!\\obj\\|\\bin\\).)+$' } |
    ForEach-Object {
        $measured = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8) | Measure-Object -Line;
        [PSCustomObject]@{ Folder = $_.FullName; LineCount = $measured.Lines };
    } |
    Measure-Object -Property LineCount -Sum
```

*Wouldn't wish anyone an AI to assist one reaching the unmaintainable  
human-incomprehensible amount of code as fast as six months rather  
in those 3 years originally.*

## Project structure

### Branching rules

The main branch is "public" with commits of two types in its "trunk":

* [commit --fixup](https://git-scm.com/docs/git-commit#:~:text=Create%20a%20new%20commit%20which%20%22fixes%20up%22)  
these are used whenever any change is intended for the latest version.  
The fixups are always on top of the whole "entry point" branch graph  
where more complex entry points include less complex ending up into  
a holistic single solution (ie. the single merge commit into public).  
    > at least one of the fixups must change the root commit for  
    proper visualiation of the rebase resulting graph.
* [rebase -i --autosquash --rebase-merges --update-refs](https://git-scm.com/docs/git-rebase#:~:text=Automatically%20squash%20commits%20with%20specially%20formatted)  
following fixup rules this command copies the whole solution  
while retaining the entry point branches structure. The resulting  
graph's top commit always remain "merged to the trunk".  

This way you'll never lose the entry point you had chosen when  
cloned or submoduled this repo. And with versioning rules below  
all the versions stay visualized with all the past fixups applied.  
In other words **the structure is consistent and immutable**.

> If used in your project code this approach acts as an extension to  
[Trunk Based Development](https://trunkbaseddevelopment.com/).  
Precisely the trunk is formed up with the fixups only while the roots  
are updated at the end of iteration making your git history to be  
self-documented and clean.

<details><summary><strong>⚠️ WIP warning ⚠️</strong></summary>

From time to time you may fetch specially prefixed wip- branches.  
Those aren't entry point branches yet so they **won't be** as safe and  
consistent to use as the already finished ones (reachable from public).

Please be careful relying on them but the same time don't miss the  
opportunity to look a little further and meet those coming soon  
modules covering the extended needs you may demanded so long or  
couldn't even imagine!

</details>

### Documenting

The main form of documenting is **commit body**es' description.  
The commits within an entry point branch may be additionally  
**split by different authors**. Authors are your teammates:

* 🏠 **Domainman** - demands tools to **speak business over the code**  
while delegating its implementation. **Forms up the framework** you own.
    > One remains an expert regardless the stack or even the  
    programming language and **develops domain modules**.
* 🧱 **Facademan** - puts the domain modules together with tools like  
**ASP.NET, OpenAPI, Configuration, Reflection, Serialization, DI**.  
Exposes APIs, forms up supported API client stacks.
    > One possesses **best knowledge of the chosen entry point** and  
    organizes its further development.
* 💾 **Persistman** - **speaks database over the code** (not the one  
~~restoring from backups~~, wish you'd never know it 😅).  
Irreplaceble when it comes to **EFCore or other persistence tools**.  
    > One **adopts, optimizes, upgrades database accessing modules**  
    (unless being constantly distracted by Domainman 🙈).
* 📦 **Streamman** - let it be either a **broker's** native client  
library or a complex proxying framework **like MassTransit** or just  
bare queue-table **outbox** - will depend on Domainman needs.  
    > One organizes **data delivery and analysis**, from  
    **cross-cutting concerns**, logging, short-circuiting, scheduling  
    to implementing specific data analysing **domain services**. 

Likewise, each entry point's example project is accompanied with  
**README.md referenced in [Entry points](#entry-points)** section below.

<details><summary>ℹ️ Studying advise</summary>

Make use of the editor's per-line git blaming abilities from  
extensions like `eamodio.gitlens`. This helps a lot to grasp reasoning  
behind an arbitrary line you're curently looking at at the right moment.

</details>

<details><summary>🎁 Bonus</summary>

You may find interesting to look at
[the last rebase todo list](bonus-last-git-rebase-todo).

It is divided into **Startup Iterations** where your teammates work  
together to build up the framework and then switch into their  
dedicated fields. Btw, exactly those iteration numbers you can  
see in the very beginning of the authored commits (1|, 2|, 3| ...).

> Btw, updating this one for each version helps following the  
branching rule of fixups to always contain one for the root commit 😉.

</details>

### Versioning

This is a boilerplate repo with consistent solution structure where  
each subsequent fixup serie results in a new version of the  
"entry point" branch graph.

In addition to current version's entry point branches there are  
similarly named **version-numbered tags**. You can find it useful for  
documenting purposes, using them for stable links in your documenation  
letting to navigate to and browse your entry point online.

This way the boilerplate becomes **safe for using in any form**, from  
simple submoduling to advanced hosting in your enterprise repo.

Motivation of this strategy is simple:

> If you treat your "business-logic" valuable - you're interested in  
**owning the framework rather than "renting" it** evenually.

### Current solution

This is a graph of the current entry point's solution aligned with  
the Startup Iterations
(more in [the last rebase todo list](bonus-last-git-rebase-todo)).

The top most commit is the most recent one tagged in the graph with  
the last entry point branch included by your current revision.  
All the entry point branches' heads are highlighted in the graph as ⏹️.

> See the detailed commit descriptions gathered in
[Commit logs](#commit-logs) section.

```mermaid
---
config:
  gitGraph:
    mainBranchName: "1|🧱: basic configurations covered"
    parallelCommits: true
    rotateCommitLabel: false
    showCommitLabel: false
---
gitGraph

%% commit type:HIGHLIGHT tag:"N|🧱|wip-full-solution" %% OEMEC
%% branch "N|🧱: OLTP EFCore multi-entity chassis" %% OEMEC
%% commit type:HIGHLIGHT tag:"N|🧱|wip-oltp-di-efcore-handle-specifications-only-the-rest-is-covered-with-record-types-composition|YourCompany.Configuration.EFCore" %% OEMEC

%% Startup Iteration 7

%% branch "7|💾: EFCore multi EntityEntry visitors" %% EMEEV
%% commit type:NORMAL tag:"7|💾|YourCompany.Configuration.EFCore" %% EMEEV

%% checkout "N|🧱: OLTP EFCore multi-entity chassis" %% EMES
%% branch "7|💾: EFCore multi-entity sorting" %% EMES
%% commit type:NORMAL tag:"7|💾|YourCompany.Configuration.EFCore" %% EMES

%% checkout "7|💾: EFCore multi-entity sorting" %% ESME
%% branch "7|💾: EFCore SortingKey multi-entity" %% ESME
%% merge "7|💾: EFCore multi EntityEntry visitors" type:NORMAL tag:"7|💾|YourCompany.Configuration.EFCore" %% ESME

%% checkout "N|🧱: OLTP EFCore multi-entity chassis" %% ODEI
%% branch "7|🧱: OLTP DI EFCore integrated" %% ODEI
%% commit type:REVERSE %% ODEI>1
%% commit type:REVERSE %% ODEI>2
%% commit type:HIGHLIGHT tag:"7|🧱|public|oltp-di-efcore-handle-specifications-only-the-rest-is-covered|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.DI.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODEI

%% branch "7|🧱: OLTP DI EFCore switch to chassis" %% ODESTC
%% commit type:HIGHLIGHT tag:"7|🧱|oltp-di-efcore-handle-specifications-only-the-rest-is-covered|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODESTC
%% commit type:NORMAL tag:"7|🧱|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODESTC

%% Startup Iteration 6

%% checkout "7|🧱: OLTP DI EFCore switch to chassis" %% OEF
%% branch "6|💾: OLTP EFCore finalized" %% OEF
%% commit type:NORMAL tag:"6|💾|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% OEF

%% Startup Iteration 5

%% checkout "6|💾: OLTP EFCore finalized" %% OER
%% branch "5|💾: OLTP EFCore readonly" %% OER
%% commit type:REVERSE %% OER>1
%% commit type:REVERSE %% OER>2
%% commit type:REVERSE %% OER>3
%% commit type:REVERSE %% OER>4
%% commit type:REVERSE %% OER>5
%% commit type:NORMAL tag:"5|💾|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% OER

%% checkout "7|🧱: OLTP DI EFCore integrated" %% ODMR
%% checkout "N|🧱: OLTP EFCore multi-entity chassis" %% ODMR
%% branch "5|🧱: OLTP DI mixed repository" %% ODMR
%% commit type:REVERSE %% ODMR>1
%% commit type:REVERSE %% ODMR>2
%% commit type:REVERSE %% ODMR>3
%% commit type:REVERSE %% ODMR>4
%% commit type:REVERSE %% ODMR>5
%% commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition" %% ODMR

%% branch "5|🧱: OLTP record composition to chassis" %% ORCTC
%% commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% ORCTC
%% commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% ORCTC

%% checkout "7|🧱: OLTP DI EFCore switch to chassis" %% ODIFR
%% branch "5|🧱: OLTP DI inheritance free repo" %% ODIFR
%% commit type:REVERSE %% ODIFR>1
%% commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance|YourCompany.OLTP.RecordsManagement.DI" %% ODIFR
%% merge "5|🧱: OLTP record composition to chassis" type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance|YourCompany.OLTP.RecordsManagement.DI" %% ODIFR

%% branch "5|🧱: OLTP DI ScopedUseCasesProvider" %% ODS
%% commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI" %% 5|🧱: OLTP DI ScopedUseCasesProvider

%% checkout "5|🧱: OLTP DI inheritance free repo" %% ODSRP
%% branch "5|🧱: OLTP DI ScopedRecordsProvider" %% ODSRP
%% commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.StateOwnership.Reflection.DI" %% ODSRP

%% checkout "5|🧱: OLTP DI inheritance free repo" %% OSTC
%% commit type:REVERSE %% OSTC<1
%% commit type:REVERSE %% OSTC<2
%% commit type:REVERSE %% OSTC<3
%% branch "5|🧱: OLTP switch to chassis" %% OSTC
%% commit type:REVERSE %% OSTC>1
%% commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis|YourCompany.OLTP.RecordsManagement|YourCompany.OLTP.RecordsManagement.DI|YourCompany.OLTP.RecordsManagement.Persistence|YourCompany.OLTP.RecordsManagement.UseCases|YourCompany.OLTP.StateOwnership" %% OSTC

%% Startup Iteration 4

%% checkout "7|💾: EFCore multi-entity sorting" %% CEPFVT
%% branch "4|💾: cache EFProperty from ValueTuple" %% CEPFVT
%% commit type:REVERSE %% CEPFVT>1
%% commit type:REVERSE %% CEPFVT>2
%% commit type:REVERSE %% CEPFVT>3
%% commit type:REVERSE %% CEPFVT>4
%% commit type:REVERSE %% CEPFVT>5
%% commit type:REVERSE %% CEPFVT>6
%% commit type:REVERSE %% CEPFVT>7
%% commit type:REVERSE %% CEPFVT>8
%% commit type:REVERSE %% CEPFVT>9
%% commit type:REVERSE %% CEPFVT>10
%% commit type:REVERSE %% CEPFVT>11
%% commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% CEPFVT

%% checkout "6|💾: OLTP EFCore finalized" %% EETSKT
%% branch "4|💾: EFEntityTypeSortingKeyTopology" %% EETSKT
%% commit type:REVERSE %% EETSKT>1
%% commit type:REVERSE %% EETSKT>2
%% commit type:REVERSE %% EETSKT>3
%% commit type:REVERSE %% EETSKT>4
%% commit type:REVERSE %% EETSKT>5
%% commit type:REVERSE %% EETSKT>6
%% commit type:REVERSE %% EETSKT>7
%% commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% EETSKT

%% checkout "5|💾: OLTP EFCore readonly" %% Startup Iteration 4
%% commit type:REVERSE %% OER 4>1
%% commit type:REVERSE %% OER 4>2
%% commit type:REVERSE %% OER 4>3

%% checkout "5|💾: OLTP EFCore readonly" %% OEIG
%% branch "4|💾: OLTP EFCore ID generation" %% OEIG
%% commit type:REVERSE %% OEIG>1
%% commit type:NORMAL tag:"4|💾|YourCompany.OLTP.StateOwnership.Reflection.EFCore" %% OEIG

%% branch "4|💾: OLTP EFCore MetadataHelper" %% OEMH
%% commit type:NORMAL tag:"4|💾|YourCompany.OLTP.StateOwnership.Reflection.EFCore" %% OEMH

%% checkout "7|💾: EFCore multi EntityEntry visitors" %% Startup Iteration 4
%% commit type:REVERSE %% EMEEV 4>1
%% commit type:REVERSE %% EMEEV 4>2
%% commit type:REVERSE %% EMEEV 4>3
%% commit type:REVERSE %% EMEEV 4>4
%% commit type:REVERSE %% EMEEV 4>5
%% commit type:REVERSE %% EMEEV 4>6
%% commit type:REVERSE %% EMEEV 4>7
%% commit type:REVERSE %% EMEEV 4>8
%% commit type:REVERSE %% EMEEV 4>9
%% commit type:REVERSE %% EMEEV 4>10
%% commit type:REVERSE %% EMEEV 4>11
%% commit type:REVERSE %% EMEEV 4>12
%% commit type:REVERSE %% EMEEV 4>13

%% checkout "5|💾: OLTP EFCore readonly" %% ESKEE
%% branch "4|💾: EFCore SortingKey ⇆ EntityEntry" %% ESKEE
%% commit type:REVERSE %% ESKEE>1
%% commit type:REVERSE %% ESKEE>2
%% commit type:REVERSE %% ESKEE>3
%% commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKEE
%% merge "7|💾: EFCore multi EntityEntry visitors" type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKEE

%% branch "4|💾: EFEntityEntryPropertiesCache" %% EEEPC
%% commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% EEEPC

%% checkout "7|💾: EFCore multi-entity sorting" %% Startup Iteration 4
%% commit type:REVERSE %% EMES 4>1
%% commit type:REVERSE %% EMES 4>2
%% commit type:REVERSE %% EMES 4>3
%% commit type:REVERSE %% EMES 4>4
%% commit type:REVERSE %% EMES 4>5
%% commit type:REVERSE %% EMES 4>6
%% commit type:REVERSE %% EMES 4>7
%% commit type:REVERSE %% EMES 4>8
%% commit type:REVERSE %% EMES 4>9
%% commit type:REVERSE %% EMES 4>10
%% commit type:REVERSE %% EMES 4>11
%% commit type:REVERSE %% EMES 4>12
%% commit type:REVERSE %% EMES 4>13
%% commit type:REVERSE %% EMES 4>14
%% commit type:REVERSE %% EMES 4>15
%% commit type:REVERSE %% EMES 4>16

%% checkout "5|💾: OLTP EFCore readonly" %% ESKR
%% branch "4|💾: EFCore SortingKey reading" %% ESKR
%% commit type:REVERSE %% ESKR>1
%% commit type:REVERSE %% ESKR>2
%% commit type:REVERSE %% ESKR>3
%% commit type:REVERSE %% ESKR>4
%% commit type:REVERSE %% ESKR>5
%% commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKR
%% merge "7|💾: EFCore multi-entity sorting" type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKR

%% checkout "5|🧱: OLTP switch to chassis" %% OLMBU
%% branch "4|🧱: OLTP LINQ may be useful" %% OLMBU
%% commit type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence.Linq" %% OLMBU
%% merge "5|💾: OLTP EFCore readonly" type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence.Linq" %% OLMBU

%% branch "4|🧱: OLTP identities are unique keys" %% OIAUK
%% commit type:REVERSE %% OIAUK>1
%% commit type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence" %% OIAUK
%% commit type:NORMAL tag:"4|🧱|YourCompany.OLTP.RecordsManagement.Persistence" %% OIAUK

%% checkout "5|🧱: OLTP record composition to chassis" %% OUCBRT
%% branch "4|🏠: OLTP use cases by record type" %% OUCBRT
%% commit type:REVERSE %% OUCBRT>1
%% commit type:REVERSE %% OUCBRT>2
%% commit type:REVERSE %% OUCBRT>3
%% commit type:REVERSE %% OUCBRT>4
%% commit type:REVERSE %% OUCBRT>5
%% commit type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-with-transactional-composition|YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition" %% OUCBRT

%% checkout "4|🏠: OLTP use cases by record type" %% EOUC
%% checkout "5|🧱: OLTP switch to chassis" %% EOUC
%% branch "4|🏠: enumerate OLTP use cases" %% EOUC
%% commit type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-repository-is-for-persistence|YourCompany.OLTP.RecordsManagement.UseCases" %% EOUC
%% merge "4|🏠: OLTP use cases by record type" type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-repository-is-for-persistence|YourCompany.OLTP.RecordsManagement.UseCases" %% EOUC

%% Startup Iteration 3

%% checkout "4|💾: EFEntityTypeSortingKeyTopology" %% Startup Iteration 3
%% commit type:REVERSE %% EETSKT 3>1
%% commit type:REVERSE %% EETSKT 3>2
%% commit type:REVERSE %% EETSKT 3>3
%% commit type:REVERSE %% EETSKT 3>4
%% commit type:REVERSE %% EETSKT 3>5
%% commit type:REVERSE %% EETSKT 3>6

%% checkout "4|💾: EFCore SortingKey ⇆ EntityEntry" %% Startup Iteration 3
%% commit type:REVERSE %% ESKEE 3>1

%% checkout "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
%% checkout "7|💾: EFCore SortingKey multi-entity" %% EFCSK
%% branch "3|💾: EFCore first-class SortingKey" %% EFCSK
%% merge "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
%% merge "4|💾: EFCore SortingKey reading" %% EFCSK
%% merge "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
%% merge "4|💾: EFEntityTypeSortingKeyTopology" %% EFCSK
%% commit type:REVERSE %% EFCSK>1
%% commit type:REVERSE %% EFCSK>2
%% commit type:REVERSE %% EFCSK>3
%% commit type:REVERSE %% EFCSK>4
%% commit type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% EFCSK

%% checkout "4|💾: cache EFProperty from ValueTuple" %% Startup Iteration 3
%% commit type:REVERSE %% CEPFVT 3>1
%% commit type:REVERSE %% CEPFVT 3>2
%% commit type:REVERSE %% CEPFVT 3>3
%% commit type:REVERSE %% CEPFVT 3>4
%% commit type:REVERSE %% CEPFVT 3>5
%% commit type:REVERSE %% CEPFVT 3>6
%% commit type:REVERSE %% CEPFVT 3>7

%% checkout "3|💾: EFCore first-class SortingKey" %% EPEC
%% branch "3|💾: EFPropertyExpressionsCache" %% EPEC
%% commit type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore" %% EPEC
%% merge "4|💾: cache EFProperty from ValueTuple" type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore" %% EPEC

%% checkout "4|🧱: OLTP identities are unique keys" %% Startup Iteration 3
%% commit type:REVERSE %% OIAUK 3>1

%% checkout "4|🏠: enumerate OLTP use cases" %% OTS
%% checkout "4|🧱: OLTP identities are unique keys" %% OTS
%% branch "3|🧱: OLTP transaction structure" %% OTS
%% merge "4|🧱: OLTP identities are unique keys" %% OTS
%% commit type:REVERSE %% OTS>1
%% commit type:REVERSE %% OTS>2
%% commit type:REVERSE %% OTS>3
%% commit type:REVERSE %% OTS>4
%% commit type:REVERSE %% OTS>5
%% commit type:REVERSE %% OTS>6
%% commit type:REVERSE %% OTS>7
%% commit type:HIGHLIGHT tag:"3|🧱|oltp-typical-transaction-structure-repository-is-up-to-you|YourCompany.OLTP.RecordsManagement" %% OTS
%% merge "4|🧱: OLTP identities are unique keys" type:HIGHLIGHT tag:"3|🧱|oltp-typical-transaction-structure-repository-is-up-to-you|YourCompany.OLTP.RecordsManagement" %% OTS

%% checkout "4|🏠: OLTP use cases by record type" %% ORTCM
%% branch "3|🏠: OLTP RecordTypesCompositionMap" %% ORTCM
%% commit type:REVERSE %% ORTCM>1
%% commit type:REVERSE %% ORTCM>2
%% commit type:REVERSE %% ORTCM>3
%% commit type:REVERSE %% ORTCM>4
%% commit type:REVERSE %% ORTCM>5
%% commit type:REVERSE %% ORTCM>6
%% commit type:REVERSE %% ORTCM>7
%% commit type:REVERSE %% ORTCM>8
%% commit type:REVERSE %% ORTCM>9
%% commit type:REVERSE %% ORTCM>10
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection" %% ORTCM

%% checkout "5|🧱: OLTP DI ScopedUseCasesProvider" %% OUCTM
%% commit type:REVERSE %% OUCTM<1
%% commit type:REVERSE %% OUCTM<2
%% commit type:REVERSE %% OUCTM<3
%% commit type:REVERSE %% OUCTM<4
%% commit type:REVERSE %% OUCTM<5
%% commit type:REVERSE %% OUCTM<6
%% commit type:REVERSE %% OUCTM<7
%% commit type:REVERSE %% OUCTM<8
%% commit type:REVERSE %% OUCTM<9
%% commit type:REVERSE %% OUCTM<10
%% commit type:REVERSE %% OUCTM<11
%% commit type:REVERSE %% OUCTM<12
%% commit type:REVERSE %% OUCTM<13
%% commit type:REVERSE %% OUCTM<14
%% commit type:REVERSE %% OUCTM<15
%% branch "3|🏠: OLTP UseCaseTypesMap" %% OUCTM
%% commit type:REVERSE %% OUCTM>1
%% commit type:REVERSE %% OUCTM>2
%% commit type:REVERSE %% OUCTM>3
%% commit type:REVERSE %% OUCTM>4
%% commit type:REVERSE %% OUCTM>5
%% commit type:REVERSE %% OUCTM>6
%% commit type:REVERSE %% OUCTM>7
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases.Reflection" %% OUCTM

%% checkout "4|🏠: enumerate OLTP use cases" %% GOUC
%% branch "3|🏠: generic OLTP use cases" %% GOUC
%% commit type:REVERSE %% GOUC>1
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases" %% GOUC
%% merge "3|🏠: OLTP UseCaseTypesMap" type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases" %% GOUC

%% checkout "4|💾: OLTP EFCore MetadataHelper" %% Startup Iteration 3
%% commit type:REVERSE %% OEMH 3>1
%% commit type:REVERSE %% OEMH 3>2
%% commit type:REVERSE %% OEMH 3>3
%% commit type:REVERSE %% OEMH 3>4
%% commit type:REVERSE %% OEMH 3>5
%% commit type:REVERSE %% OEMH 3>6
%% commit type:REVERSE %% OEMH 3>7
%% commit type:REVERSE %% OEMH 3>8
%% commit type:REVERSE %% OEMH 3>9
%% commit type:REVERSE %% OEMH 3>10
%% commit type:REVERSE %% OEMH 3>11
%% commit type:REVERSE %% OEMH 3>12
%% commit type:REVERSE %% OEMH 3>13
%% commit type:REVERSE %% OEMH 3>14

%% checkout "3|🏠: OLTP RecordTypesCompositionMap" %% ORTM
%% checkout "5|🧱: OLTP DI ScopedRecordsProvider" %% ORTM
%% branch "3|🏠: OLTP RecordTypesMap" %% ORTM
%% merge "3|🏠: OLTP RecordTypesCompositionMap" %% ORTM
%% commit type:REVERSE %% ORTM>1
%% commit type:REVERSE %% ORTM>2
%% commit type:REVERSE %% ORTM>3
%% commit type:REVERSE %% ORTM>4
%% commit type:REVERSE %% ORTM>5
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM
%% merge "3|🏠: OLTP RecordTypesCompositionMap" type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM
%% merge "4|💾: OLTP EFCore MetadataHelper" type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM

%% Startup Iteration 2

%% checkout "N|🧱: wip switch to chassis" %% EHMR
%% branch "2|💾: EFCore hosting migration run" %% EHMR
%% commit type:REVERSE %% EHMR>1
%% commit type:REVERSE %% EHMR>2
%% commit type:REVERSE %% EHMR>3
%% commit type:REVERSE %% EHMR>4
%% commit type:REVERSE %% EHMR>5
%% commit type:REVERSE %% EHMR>6
%% commit type:REVERSE %% EHMR>7
%% commit type:REVERSE %% EHMR>8
%% commit type:REVERSE %% EHMR>9
%% commit type:REVERSE %% EHMR>10
%% commit type:REVERSE %% EHMR>11
%% commit type:REVERSE %% EHMR>12
%% commit type:REVERSE %% EHMR>13
%% commit type:REVERSE %% EHMR>14
%% commit type:REVERSE %% EHMR>15
%% commit type:REVERSE %% EHMR>16
%% commit type:REVERSE %% EHMR>17
%% commit type:REVERSE %% EHMR>18
%% commit type:REVERSE %% EHMR>19
%% commit type:REVERSE %% EHMR>20
%% commit type:REVERSE %% EHMR>21
%% commit type:REVERSE %% EHMR>22
%% commit type:REVERSE %% EHMR>23
%% commit type:REVERSE %% EHMR>24
%% commit type:REVERSE %% EHMR>25
%% commit type:REVERSE %% EHMR>26
%% commit type:REVERSE %% EHMR>27
%% commit type:REVERSE %% EHMR>28
%% commit type:REVERSE %% EHMR>29
%% commit type:REVERSE %% EHMR>30
%% commit type:REVERSE %% EHMR>31
%% commit type:REVERSE %% EHMR>32
%% commit type:REVERSE %% EHMR>33
%% commit type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore.Hosting" %% EHMR

%% checkout "6|💾: OLTP EFCore finalized" %% ECTTGS
%% commit type:REVERSE %% ECTTGS<1
%% commit type:REVERSE %% ECTTGS<2
%% commit type:REVERSE %% ECTTGS<3
%% commit type:REVERSE %% ECTTGS<4
%% commit type:REVERSE %% ECTTGS<5
%% commit type:REVERSE %% ECTTGS<6
%% commit type:REVERSE %% ECTTGS<7
%% commit type:REVERSE %% ECTTGS<8
%% commit type:REVERSE %% ECTTGS<9
%% commit type:REVERSE %% ECTTGS<10
%% commit type:REVERSE %% ECTTGS<11
%% commit type:REVERSE %% ECTTGS<12
%% commit type:REVERSE %% ECTTGS<13
%% commit type:REVERSE %% ECTTGS<14
%% commit type:REVERSE %% ECTTGS<15
%% commit type:REVERSE %% ECTTGS<16
%% commit type:REVERSE %% ECTTGS<17
%% commit type:REVERSE %% ECTTGS<18
%% commit type:REVERSE %% ECTTGS<19
%% commit type:REVERSE %% ECTTGS<20
%% commit type:REVERSE %% ECTTGS<21
%% commit type:REVERSE %% ECTTGS<22
%% commit type:REVERSE %% ECTTGS<23
%% commit type:REVERSE %% ECTTGS<24
%% commit type:REVERSE %% ECTTGS<25
%% branch "2|💾: EFChangeTrackerTrackGraphStrategy" %% ECTTGS
%% commit type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore" %% ECTTGS

%% checkout "6|💾: OLTP EFCore finalized" %% EPL
%% branch "2|💾: EFCore pessimistic locking" %% EPL
%% commit type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% EPL

%% checkout "4|💾: EFEntityEntryPropertiesCache" %% BPE
%% branch "2|💾: basic pluggable EFCore" %% BPE

%% checkout "2|💾: EFChangeTrackerTrackGraphStrategy" %% BPE
%% commit type:REVERSE %% ECTTGS BPE>1

%% checkout "2|💾: basic pluggable EFCore" %% BPE
%% merge "3|💾: EFPropertyExpressionsCache" %% BPE
%% merge "2|💾: EFCore pessimistic locking" %% BPE
%% merge "2|💾: EFChangeTrackerTrackGraphStrategy" %% BPE
%% merge "2|💾: EFChangeTrackerTrackGraphStrategy" type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% BPE
%% merge "2|💾: EFCore hosting migration run" type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% BPE

%% checkout "3|🏠: OLTP RecordTypesCompositionMap" %% TCM
%% branch "2|🧱: TypesCompositionMap" %% TCM
%% commit type:REVERSE %% TCM>1
%% commit type:REVERSE %% TCM>2
%% commit type:REVERSE %% TCM>3
%% commit type:REVERSE %% TCM>4
%% commit type:REVERSE %% TCM>5
%% commit type:REVERSE %% TCM>6
%% commit type:REVERSE %% TCM>7
%% commit type:REVERSE %% TCM>8
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TCM

%% checkout "3|🏠: OLTP UseCaseTypesMap" %% TAH
%% commit type:REVERSE %% TAH<1
%% commit type:REVERSE %% TAH<2
%% branch "2|🧱: TypeAbstractionsHelper" %% TAH
%% commit type:REVERSE %% TAH>1
%% commit type:REVERSE %% TAH>2
%% commit type:REVERSE %% TAH>3
%% commit type:REVERSE %% TAH>4
%% commit type:REVERSE %% TAH>5
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TAH
%% merge "2|🧱: TypesCompositionMap" type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TAH

%% checkout "4|💾: EFCore SortingKey reading" %% VTH
%% commit type:REVERSE %% VTH<1
%% commit type:REVERSE %% VTH<2
%% commit type:REVERSE %% VTH<3
%% commit type:REVERSE %% VTH<4
%% branch "2|🧱: ValueTupleHelper" %% VTH
%% merge "4|💾: cache EFProperty from ValueTuple" %% VTH
%% commit type:REVERSE %% VTH>1
%% commit type:REVERSE %% VTH>2
%% commit type:REVERSE %% VTH>3
%% commit type:REVERSE %% VTH>4
%% commit type:REVERSE %% VTH>5
%% commit type:REVERSE %% VTH>6
%% commit type:REVERSE %% VTH>7
%% commit type:REVERSE %% VTH>8
%% commit type:REVERSE %% VTH>9
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% VTH

%% checkout "5|🧱: OLTP DI ScopedRecordsProvider" %% DCH
%% branch "2|🧱: DIConstructorHelper" %% DCH
%% commit type:REVERSE %% DCH>1
%% commit type:REVERSE %% DCH>2
%% commit type:REVERSE %% DCH>3
%% commit type:REVERSE %% DCH>4
%% commit type:REVERSE %% DCH>5
%% commit type:REVERSE %% DCH>6
%% commit type:REVERSE %% DCH>7
%% commit type:REVERSE %% DCH>8
%% commit type:REVERSE %% DCH>9
%% commit type:REVERSE %% DCH>10
%% commit type:REVERSE %% DCH>11
%% commit type:REVERSE %% DCH>12
%% commit type:REVERSE %% DCH>13
%% commit type:REVERSE %% DCH>14
%% commit type:REVERSE %% DCH>15
%% commit type:REVERSE %% DCH>16
%% commit type:REVERSE %% DCH>17
%% commit type:REVERSE %% DCH>18
%% commit type:REVERSE %% DCH>19
%% commit type:REVERSE %% DCH>20
%% commit type:REVERSE %% DCH>21
%% commit type:REVERSE %% DCH>22
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection.DI" %% DCH

%% checkout "3|💾: EFPropertyExpressionsCache" %% Startup Iteration 2
%% commit type:REVERSE %% EPEC 2>1

%% checkout "2|🧱: TypeAbstractionsHelper" %% GMH
%% checkout "4|💾: EFEntityEntryPropertiesCache" %% GMH
%% branch "2|🧱: GetMemberHelper" %% GMH
%% merge "3|💾: EFPropertyExpressionsCache" %% GMH
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% GMH
%% merge "2|🧱: DIConstructorHelper" type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% GMH

%% checkout "3|🏠: generic OLTP use cases" %% ATL
%% branch "2|🧱: AwaitTasksList" %% ATL
%% commit type:REVERSE %% ATL>1
%% commit type:REVERSE %% ATL>2
%% commit type:REVERSE %% ATL>3
%% commit type:REVERSE %% ATL>4
%% commit type:REVERSE %% ATL>5
%% commit type:NORMAL tag:"2|🧱|YourCompany.Threading" %% ATL

%% checkout "4|🧱: OLTP identities are unique keys" %% SH
%% branch "2|🧱: StructHelper" %% SH
%% commit type:REVERSE %% SH>1
%% commit type:REVERSE %% SH>2
%% commit type:REVERSE %% SH>3
%% commit type:REVERSE %% SH>4
%% commit type:REVERSE %% SH>5
%% commit type:REVERSE %% SH>6
%% commit type:REVERSE %% SH>7
%% commit type:REVERSE %% SH>8
%% commit type:REVERSE %% SH>9
%% commit type:REVERSE %% SH>10
%% commit type:REVERSE %% SH>11
%% commit type:REVERSE %% SH>12
%% commit type:REVERSE %% SH>13
%% commit type:REVERSE %% SH>14
%% commit type:REVERSE %% SH>15
%% commit type:REVERSE %% SH>16
%% commit type:REVERSE %% SH>17
%% commit type:REVERSE %% SH>18
%% commit type:NORMAL tag:"2|🧱|YourCompany.CompilerServices" %% SH

%% checkout "3|🧱: OLTP transaction structure" %% Startup Iteration 2
%% commit type:REVERSE %% OTS 2>1
%% commit type:REVERSE %% OTS 2>2
%% commit type:REVERSE %% OTS 2>3
%% commit type:REVERSE %% OTS 2>4
%% commit type:REVERSE %% OTS 2>5
%% commit type:REVERSE %% OTS 2>6
%% commit type:REVERSE %% OTS 2>7
%% commit type:REVERSE %% OTS 2>8
%% commit type:REVERSE %% OTS 2>9
%% commit type:REVERSE %% OTS 2>10
%% commit type:REVERSE %% OTS 2>11
%% commit type:REVERSE %% OTS 2>12

%% checkout "3|🏠: generic OLTP use cases" %% OTILS
%% checkout "3|🧱: OLTP transaction structure" %% OTILS
%% branch "2|🏠: OLTP transaction is limited size" %% OTILS
%% commit type:HIGHLIGHT tag:"2|🏠|oltp-ways-to-access-size-limited-record-batch|YourCompany.OLTP.RecordsManagement" %% OTILS
%% merge "3|🧱: OLTP transaction structure" type:HIGHLIGHT tag:"2|🏠|oltp-ways-to-access-size-limited-record-batch|YourCompany.OLTP.RecordsManagement" %% OTILS

%% Startup Iteration 1

%% checkout "MERGED" %% BCC
%% checkout "2|💾: basic pluggable EFCore" %% BCC
%% branch "1|🧱: basic configurations covered" %% BCC
%% commit type:REVERSE %% BCC>1
%% commit type:REVERSE %% BCC>2
%% commit type:REVERSE %% BCC>3
%% commit type:REVERSE %% BCC>4
commit type:HIGHLIGHT tag:"1|🧱|configuration-entry-assembly-rotating-secrets-scaling-plugins|YourCompany.Configuration" %% BCC

%% checkout "3|🏠: OLTP RecordTypesCompositionMap" %% OTC
%% commit type:REVERSE %% OTC<1
%% commit type:REVERSE %% OTC<2
%% commit type:REVERSE %% OTC<3
%% commit type:REVERSE %% OTC<4
%% commit type:REVERSE %% OTC<5
%% commit type:REVERSE %% OTC<6
%% commit type:REVERSE %% OTC<7
%% commit type:REVERSE %% OTC<8
%% commit type:REVERSE %% OTC<9
%% commit type:REVERSE %% OTC<10
%% commit type:REVERSE %% OTC<11
%% commit type:REVERSE %% OTC<12
%% commit type:REVERSE %% OTC<13
%% commit type:REVERSE %% OTC<14
%% commit type:REVERSE %% OTC<15
%% commit type:REVERSE %% OTC<16
%% commit type:REVERSE %% OTC<17
%% commit type:REVERSE %% OTC<18
%% commit type:REVERSE %% OTC<19
%% commit type:REVERSE %% OTC<20
%% branch "1|🏠: OLTP transactional composition" %% OTC
%% commit type:REVERSE %% OTC>1
%% commit type:REVERSE %% OTC>2
%% commit type:REVERSE %% OTC>3
%% commit type:REVERSE %% OTC>4
%% commit type:REVERSE %% OTC>5
%% commit type:REVERSE %% OTC>6
%% commit type:REVERSE %% OTC>7
%% commit type:REVERSE %% OTC>8
%% commit type:REVERSE %% OTC>9
%% commit type:REVERSE %% OTC>10
%% commit type:REVERSE %% OTC>11
%% commit type:REVERSE %% OTC>12
%% commit type:REVERSE %% OTC>13
%% commit type:REVERSE %% OTC>14
%% commit type:NORMAL tag:"1|🏠|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% OTC

%% checkout "N|🧱: wip switch to chassis" %% OEP
%% commit type:REVERSE %% OEP<1
%% commit type:REVERSE %% OEP<2
%% commit type:REVERSE %% OEP<3
%% commit type:REVERSE %% OEP<4
%% commit type:REVERSE %% OEP<5
%% commit type:REVERSE %% OEP<6
%% commit type:REVERSE %% OEP<7
%% commit type:REVERSE %% OEP<8
%% commit type:REVERSE %% OEP<9
%% commit type:REVERSE %% OEP<10
%% commit type:REVERSE %% OEP<11
%% commit type:REVERSE %% OEP<12
%% commit type:REVERSE %% OEP<13
%% commit type:REVERSE %% OEP<14
%% commit type:REVERSE %% OEP<15
%% commit type:REVERSE %% OEP<16
%% commit type:REVERSE %% OEP<17
%% commit type:REVERSE %% OEP<18
%% commit type:REVERSE %% OEP<19
%% branch "1|🏠: OLTP events producing" %% OEP
%% commit type:REVERSE %% OEP>1
%% commit type:REVERSE %% OEP>2
%% commit type:REVERSE %% OEP>3
%% commit type:REVERSE %% OEP>4
%% commit type:REVERSE %% OEP>5
%% commit type:REVERSE %% OEP>6
%% commit type:REVERSE %% OEP>7
%% commit type:REVERSE %% OEP>8
%% commit type:REVERSE %% OEP>9
%% commit type:REVERSE %% OEP>10
%% commit type:REVERSE %% OEP>11
%% commit type:REVERSE %% OEP>12
%% commit type:REVERSE %% OEP>13
%% commit type:REVERSE %% OEP>14
%% commit type:REVERSE %% OEP>15
%% commit type:REVERSE %% OEP>16
%% commit type:REVERSE %% OEP>17
%% commit type:REVERSE %% OEP>18
%% commit type:REVERSE %% OEP>19
%% commit type:REVERSE %% OEP>20
%% commit type:REVERSE %% OEP>21
%% commit type:REVERSE %% OEP>22
%% commit type:REVERSE %% OEP>23
%% commit type:REVERSE %% OEP>24
%% commit type:REVERSE %% OEP>25
%% commit type:REVERSE %% OEP>26
%% commit type:REVERSE %% OEP>27
%% commit type:REVERSE %% OEP>28
%% commit type:REVERSE %% OEP>29
%% commit type:REVERSE %% OEP>30
%% commit type:REVERSE %% OEP>31
%% commit type:REVERSE %% OEP>32
%% commit type:REVERSE %% OEP>33
%% commit type:REVERSE %% OEP>34
%% commit type:REVERSE %% OEP>35
%% commit type:REVERSE %% OEP>36
%% commit type:NORMAL tag:"1|🏠|YourCompany.OLTP.StateOwnership.EventsProducing" %% OEP

%% checkout "2|🏠: OLTP transaction is limited size" %% Startup Iteration 1
%% commit type:REVERSE %% OTILS 1>1
%% commit type:REVERSE %% OTILS 1>2
%% commit type:REVERSE %% OTILS 1>3

%% checkout MERGED %% THOOIT
%% checkout "2|🏠: OLTP transaction is limited size" %% THOOIT
%% checkout "3|🏠: OLTP RecordTypesMap" %% THOOIT
%% branch "1|🏠: the heart of OLTP is transaction" %% THOOIT
%% merge "2|🏠: OLTP transaction is limited size" %% THOOIT
%% merge "1|🏠: OLTP events producing" %% THOOIT
%% merge "1|🏠: OLTP transactional composition" %% THOOIT
%% merge "N|🧱: wip switch to chassis" %% THOOIT
%% commit type:HIGHLIGHT tag:"1|🏠|oltp-basic-object-model-the-rest-is-up-to-you|YourCompany.OLTP.StateOwnership" %% THOOIT
%% merge "2|🏠: OLTP transaction is limited size" type:HIGHLIGHT tag:"1|🏠|oltp-basic-object-model-the-rest-is-up-to-you|YourCompany.OLTP.StateOwnership" %% THOOIT
```

## Contact me

Still reading? 🥳 Hopefully the above and what expects you next  
resonates to your professional experience 🤞

With any comments, thanks or other feedback please contact me at  
gzorik.us@gmail.com.

Looking forward to a productive conversation with you 👀

## Entry points

Now let's dive in into the entry point branches available. It's sorted  
by complexity decrease top to bottom. The first block is the current  
solution, the latest is a template.

<!-- ### Branch: oltp-di-efcore-handle-specifications-only-the-rest-is-covered

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-di-efcore-handle-specifications-only-the-rest-is-covered END -->

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition END -->

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance END -->

<!-- ### Branch: oltp-segregate-use-cases-with-transactional-composition

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-segregate-use-cases-with-transactional-composition END -->

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis END -->

<!-- ### Branch: oltp-segregate-persistence-repository-is-for-use-cases

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-segregate-persistence-repository-is-for-use-cases END -->

<!-- ### Branch: oltp-segregate-use-cases-repository-is-for-persistence

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-segregate-use-cases-repository-is-for-persistence END -->

<!-- ### Branch: oltp-typical-transaction-structure-repository-is-up-to-you

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-typical-transaction-structure-repository-is-up-to-you END -->

<!-- ### Branch: oltp-ways-to-access-size-limited-record-batch

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-ways-to-access-size-limited-record-batch END -->

### Branch: configuration-entry-assembly-rotating-secrets-scaling-plugins

> Files: 10 | Lines: 637  
Pros: not tied to typical component types, the most common  
Cons: you may find another set of conventions more suitable

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🧱: basic configurations covered](#commit-1-basic-configurations-covered) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-basic-configurations-covered) </kbd> | <kbd>YourCompany.Configuration (+637 lines)</kbd> |

<!-- ### Branch: configuration-entry-assembly-rotating-secrets-scaling-plugins END -->

<!-- ### Branch: oltp-basic-object-model-the-rest-is-up-to-you

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: oltp-basic-object-model-the-rest-is-up-to-you END -->

### Branch: template-entry-point-branch-name

> Files: ### | Lines: #####  
Pros: why do you choose this entry point  
Cons: why it might be not enough for you

| Log | Examples | Modules |
|-|-|-|
| [replace_with_each_included_commit_subject](#commit-with-lower-case-subject) | <kbd> [at‑least‑one‑example‑solution‑name](examples/at-least-one-example-solution-per-commit/README.md#commit-with-lower-case-subject) </kbd><br><kbd> [extra‑example‑solution‑name](examples/extra-example-solution-name/README.md#commit-with-lower-case-subject) </kbd> | <kbd>YourCompany.Framework.Assembly.Name1 (+diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.Name2 (-diff lines)</kbd><br><kbd>YourCompany.Framework.Assembly.NameN (0 lines)</kbd> |

<!-- ### Branch: template-entry-point-branch-name END -->

> To avoid manual work filling that table, run the
[script](scripts/CommitsTableMdToClipboard.ps1) while staying  
on break when rebasing is in progress and then paste the table.

## Commit logs

> 👇 Replaced or appended in ordered manner by the [script](scripts/CommitLogsToReadmeMd.ps1).

<!-- ### Commit: 1|🏠: the heart of OLTP is transaction

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 1|🏠: the heart of OLTP is transaction END -->

<!-- ### Commit: 1|🏠: OLTP events producing

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 1|🏠: OLTP events producing END -->

<!-- ### Commit: 1|🏠: OLTP transactional composition

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 1|🏠: OLTP transactional composition END -->

### Commit: 1|🧱: basic configurations covered

<table><tbody><tr><td>

10 files changed, 637 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration/</sub></sub><br>
<kbd> +++++++ 182 |⁠ [EnvironmentConventions.cs                                                                                      ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>
<kbd>   +++++ 122 |⁠ [YourCompanyPluginsLoader.cs                                                                                    ](src/YourCompany.Configuration/YourCompanyPluginsLoader.cs)</kbd><br>
<kbd>   +++++ 122 |⁠ [ConfigurationExtensions.cs                                                                                     ](src/YourCompany.Configuration/ConfigurationExtensions.cs)</kbd><br>
<kbd>     +++ 75  |⁠ [YourCompanyAssemblyLoadContext.cs                                                                              ](src/YourCompany.Configuration/YourCompanyAssemblyLoadContext.cs)</kbd><br>
<kbd>      ++ 31  |⁠ [AssemblyExtensions.cs                                                                                          ](src/YourCompany.Configuration/AssemblyExtensions.cs)</kbd><br>
<kbd>       + 29  |⁠ [GitHelper.cs                                                                                                   ](src/YourCompany.Configuration/GitHelper.cs)</kbd><br>
<kbd>       + 11  |⁠ [YourCompanyPluginsLoadingContext.cs                                                                            ](src/YourCompany.Configuration/YourCompanyPluginsLoadingContext.cs)</kbd><br>
<kbd>       + 9   |⁠ [YourCompanyPluginLoadingConfiguration.cs                                                                       ](src/YourCompany.Configuration/YourCompanyPluginLoadingConfiguration.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration/KeyFileToBase64/</sub></sub><br>
<kbd>      ++ 41  |⁠ [KeyFileToBase64ConfigurationProvider.cs                                                                        ](src/YourCompany.Configuration/KeyFileToBase64/KeyFileToBase64ConfigurationProvider.cs)</kbd><br>
<kbd>       + 15  |⁠ [KeyFileToBase64ConfigurationSource.cs                                                                          ](src/YourCompany.Configuration/KeyFileToBase64/KeyFileToBase64ConfigurationSource.cs)</kbd><br>

⚠️ make sure your application entry project (one per solution) has  
reference to Microsoft.Extensions.Configuration.UserSecrets package  
and sets unique `UserSecretsId` beneath the `TargetFramework` ⚠️ that  
serves as a marker to find an entry assembly which is a main convention.

`EnvironmentConventions` contains constants explaining the basics.  
As of unusual features is `GetYourCompanyInfraObjectNamesPrefix`  
`IConfiguration` extension method which allows to "switch environment  
without switching infrastructure". This might be useful to either  
utilize integrated infrastructure's own scaling abilities  
(e.g. namespaces/topics) or simply organize zero-configuration start  
for your developers letting to share the same testing environment that  
comes out very handy sometimes.

As of plugins - one decision point is being "collectible". It's not  
that relevant nowadays, since we work in containerized environments  
and therefore we skip using it. For simplicity it's enough to use  
single loading context per extension point, where it loads those of  
assemblies required only which are not reachable down through the  
entry assembly's dependencies.

</td></tr></tbody></table>

<!-- ### Commit: 1|🧱: basic configurations covered END -->

<!-- ### Commit: 2|🏠: OLTP transaction is limited size

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🏠: OLTP transaction is limited size END -->

<!-- ### Commit: 2|🧱: StructHelper

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: StructHelper END -->

<!-- ### Commit: 2|🧱: AwaitTasksList

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: AwaitTasksList END -->

### Commit: 2|🧱: GetMemberHelper

<table><tbody><tr><td>

1 file changed, 57 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection/</sub></sub><br>
<kbd> +++++++ 57  |⁠ [GetMemberHelper.cs                                                                                             ](src/YourCompany.Reflection/GetMemberHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: GetMemberHelper END -->

<!-- ### Commit: 2|🧱: DIConstructorHelper

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: DIConstructorHelper END -->

<!-- ### Commit: 2|🧱: ValueTupleHelper

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: ValueTupleHelper END -->

<!-- ### Commit: 2|🧱: TypeAbstractionsHelper

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: TypeAbstractionsHelper END -->

<!-- ### Commit: 2|🧱: TypesCompositionMap

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: TypesCompositionMap END -->

### Commit: 2|💾: basic pluggable EFCore

<table><tbody><tr><td>

11 files changed, 681 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd> +++++++ 145 |⁠ [YourCompanyDbContext.DbObjectsPrefixing.cs                                                                     ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.DbObjectsPrefixing.cs)</kbd><br>
<kbd>   +++++ 117 |⁠ [YourCompanyDbContext.cs                                                                                        ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.cs)</kbd><br>
<kbd>    ++++ 81  |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<kbd>      ++ 34  |⁠ [YourCompanyDbContextConfiguratorsLoadingContext.cs                                                             ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfiguratorsLoadingContext.cs)</kbd><br>
<kbd>       + 22  |⁠ [YourCompanyDbContextConfigurator.cs                                                                            ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfigurator.cs)</kbd><br>
<kbd>       + 13  |⁠ [ConfigurationExtensions.cs                                                                                     ](src/YourCompany.Configuration.EFCore/ConfigurationExtensions.cs)</kbd><br>
<kbd>       + 11  |⁠ [YourCompanyDbContextConfiguration.cs                                                                           ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfiguration.cs)</kbd><br>
<kbd>       + 6   |⁠ [YourCompanyDbContextFactoryLoadingConfiguration.cs                                                             ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactoryLoadingConfiguration.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.PostgreSQL/</sub></sub><br>
<kbd>   +++++ 115 |⁠ [PrefixedObjectNamesShortener.cs                                                                                ](src/YourCompany.Configuration.EFCore.PostgreSQL/PrefixedObjectNamesShortener.cs)</kbd><br>
<kbd>     +++ 66  |⁠ [NpgsqlDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlDbContextConfigurator.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.Sqlite/</sub></sub><br>
<kbd>     +++ 71  |⁠ [SqliteDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.Sqlite/SqliteDbContextConfigurator.cs)</kbd><br>

Firstly it supports both `RunTime` and `DesignTime` factories which  
are usually troublesome to combine. See detailed in the example how to  
make your migrations target this shared `YourCompanyDbContext`.

As of `YourCompanyDbContext` it accepts `TConfiguration` what allows  
you to have multiple configurations and models respectively but  
its both uncommon and not encouraged in practice (i.e. separate apps).

Also there is `ConfiguredUtcNow` which is useful in testing. And ofc  
we support `GetYourCompanyInfraObjectNamesPrefix` by prefixing DB  
objects within the connection string configured schema.

For demo provider plugins we've chosen PostgreSQL and Sqlite.

</td></tr></tbody></table>

<!-- ### Commit: 2|💾: basic pluggable EFCore END -->

<!-- ### Commit: 2|💾: EFCore pessimistic locking

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|💾: EFCore pessimistic locking END -->

<!-- ### Commit: 2|💾: EFChangeTrackerTrackGraphStrategy

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|💾: EFChangeTrackerTrackGraphStrategy END -->

<!-- ### Commit: 2|💾: EFCore hosting migration run

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 2|💾: EFCore hosting migration run END -->

<!-- ### Commit: 3|🏠: OLTP RecordTypesMap

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP RecordTypesMap END -->

<!-- ### Commit: 3|🏠: generic OLTP use cases

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: generic OLTP use cases END -->

<!-- ### Commit: 3|🏠: OLTP UseCaseTypesMap

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP UseCaseTypesMap END -->

<!-- ### Commit: 3|🏠: OLTP RecordTypesCompositionMap

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP RecordTypesCompositionMap END -->

<!-- ### Commit: 3|🧱: OLTP transaction structure

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 3|🧱: OLTP transaction structure END -->

### Commit: 3|💾: EFPropertyExpressionsCache

<table><tbody><tr><td>

2 files changed, 87 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/ExpressionsCaching/</sub></sub><br>
<kbd> +++++++ 50  |⁠ [EFPropertyExpressionsCache.FromParameter.cs                                                                    ](src/YourCompany.Configuration.EFCore/ExpressionsCaching/EFPropertyExpressionsCache.FromParameter.cs)</kbd><br>
<kbd>   +++++ 37  |⁠ [EFPropertyExpressionsCache.cs                                                                                  ](src/YourCompany.Configuration.EFCore/ExpressionsCaching/EFPropertyExpressionsCache.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 3|💾: EFPropertyExpressionsCache END -->

### Commit: 3|💾: EFCore first-class SortingKey

<table><tbody><tr><td>

36 files changed, 2464 insertions(+), 2 deletions(-)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd> +++++++ 177 |⁠ [SortingKeyPropertiesVisitor.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyPropertiesVisitor.cs)</kbd><br>
<kbd>  ++++++ 176 |⁠ [SortingKeyPredicatesBuilder.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyPredicatesBuilder.cs)</kbd><br>
<kbd>  ++++++ 160 |⁠ [SortingKey.ValueHolding.ConvertedFrom.cs                                                                       ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.ValueHolding.ConvertedFrom.cs)</kbd><br>
<kbd>  ++++++ 155 |⁠ [SortingKey.ValueHolding.cs                                                                                     ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.ValueHolding.cs)</kbd><br>
<kbd>   +++++ 139 |⁠ [ICollationAwareModelProvider.cs                                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ICollationAwareModelProvider.cs)</kbd><br>
<kbd>    ++++ 104 |⁠ [SortingKeyBuilder.OrElse.ComparableValue.cs                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyBuilder.OrElse.ComparableValue.cs)</kbd><br>
<kbd>    ++++ 100 |⁠ [SortingKey.Building.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Building.cs)</kbd><br>
<kbd>    ++++ 95  |⁠ [SortingKeyTopology.PropertiesOwner.cs                                                                          ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopology.PropertiesOwner.cs)</kbd><br>
<kbd>    ++++ 94  |⁠ [SortingKeyBuilder.ComparableValue.cs                                                                           ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyBuilder.ComparableValue.cs)</kbd><br>
<kbd>    ++++ 94  |⁠ [SortingKey.ToString.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.ToString.cs)</kbd><br>
<kbd>     +++ 86  |⁠ [SortingKey.Querying.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Querying.cs)</kbd><br>
<kbd>     +++ 76  |⁠ [OrElseEqualityKeys.Querying.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.Querying.cs)</kbd><br>
<kbd>     +++ 73  |⁠ [ICollationCompatibleComparersProvider.cs                                                                       ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ICollationCompatibleComparersProvider.cs)</kbd><br>
<kbd>     +++ 69  |⁠ [SortingKey.Comparison.cs                                                                                       ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Comparison.cs)</kbd><br>
<kbd>     +++ 69  |⁠ [SortingKeyPredicatesBuilder.SingleEntityQuery.cs                                                               ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyPredicatesBuilder.SingleEntityQuery.cs)</kbd><br>
<kbd>     +++ 66  |⁠ [SortingKey.cs                                                                                                  ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.cs)</kbd><br>
<kbd>     +++ 63  |⁠ [OrElseEqualityKeys.cs                                                                                          ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.cs)</kbd><br>
<kbd>     +++ 60  |⁠ [SortingKeyTopologyExtensions.cs                                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopologyExtensions.cs)</kbd><br>
<kbd>      ++ 50  |⁠ [SortingKeyQueries.cs                                                                                           ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyQueries.cs)</kbd><br>
<kbd>      ++ 47  |⁠ [SortingKeyTopology.Comparer.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopology.Comparer.cs)</kbd><br>
<kbd>      ++ 44  |⁠ [OrElseEqualityKeys.ToString.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.ToString.cs)</kbd><br>
<kbd>      ++ 40  |⁠ [SortingKeyTopology.cs                                                                                          ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopology.cs)</kbd><br>
<kbd>      ++ 35  |⁠ [SortingKeyBuilder.cs                                                                                           ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyBuilder.cs)</kbd><br>
<kbd>      ++ 32  |⁠ [SortingKeyBuilder.OrElse.cs                                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyBuilder.OrElse.cs)</kbd><br>
<kbd>       + 21  |⁠ [SortingKey.Visiting.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Visiting.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/</sub></sub><br>
<kbd>   +++++ 123 |⁠ [CollationAwareSortingSingleModelProvider.cs                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/CollationAwareSortingSingleModelProvider.cs)</kbd><br>
<kbd>       + 18  |⁠ [ModelExtensions.cs                                                                                             ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/ModelExtensions.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd>     +++ 80  |⁠ [YourCompanyDbContext.SingleEntityQueries.cs                                                                    ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.SingleEntityQueries.cs)</kbd><br>
<kbd>       + 19  |⁠ [YourCompanyDbContext.cs                                                                                        ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.cs)</kbd><br>
<kbd>       + 12  |⁠ [YourCompanyDbContextConfiguratorsLoadingContext.cs                                                             ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfiguratorsLoadingContext.cs)</kbd><br>
<kbd>       + 9   |⁠ [YourCompanyDbContextConfigurator.cs                                                                            ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfigurator.cs)</kbd><br>
<kbd>       + 2   |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.PostgreSQL/</sub></sub><br>
<kbd>      ++ 33  |⁠ [NpgsqlDbContextConfigurator.CollationAwareSorting.cs                                                           ](src/YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlDbContextConfigurator.CollationAwareSorting.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [NpgsqlDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlDbContextConfigurator.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.Sqlite/</sub></sub><br>
<kbd>      ++ 33  |⁠ [SqliteDbContextConfigurator.CollationAwareSorting.cs                                                           ](src/YourCompany.Configuration.EFCore.Sqlite/SqliteDbContextConfigurator.CollationAwareSorting.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [SqliteDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.Sqlite/SqliteDbContextConfigurator.cs)</kbd><br>

This one is very missing out-of-the-box in EFCore. This replaces the  
need to manually compose `OrderBy` `ThenBy` expressions allowing  
comprehensive approaches to be implemented upon from query building  
and keyset pagination to advanced change tracking operations.

Another covered problem is database strings collation having no analog  
out-of-the-box neither in .NET nor in EFCore. In such circumstances  
it is proper to delegate the missing logic to database provider  
plugins which is exactly what was done.

</td></tr></tbody></table>

<!-- ### Commit: 3|💾: EFCore first-class SortingKey END -->

<!-- ### Commit: 4|🏠: enumerate OLTP use cases

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|🏠: enumerate OLTP use cases END -->

<!-- ### Commit: 4|🏠: OLTP use cases by record type

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|🏠: OLTP use cases by record type END -->

<!-- ### Commit: 4|🧱: OLTP identities are unique keys

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|🧱: OLTP identities are unique keys END -->

<!-- ### Commit: 4|🧱: OLTP LINQ may be useful

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|🧱: OLTP LINQ may be useful END -->

<!-- ### Commit: 4|💾: EFCore SortingKey reading

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFCore SortingKey reading END -->

<!-- ### Commit: 4|💾: EFEntityEntryPropertiesCache

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFEntityEntryPropertiesCache END -->

<!-- ### Commit: 4|💾: EFCore SortingKey ⇆ EntityEntry

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFCore SortingKey ⇆ EntityEntry END -->

<!-- ### Commit: 4|💾: OLTP EFCore MetadataHelper

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: OLTP EFCore MetadataHelper END -->

<!-- ### Commit: 4|💾: OLTP EFCore ID generation

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: OLTP EFCore ID generation END -->

<!-- ### Commit: 4|💾: EFEntityTypeSortingKeyTopology

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFEntityTypeSortingKeyTopology END -->

<!-- ### Commit: 4|💾: cache EFProperty from ValueTuple

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: cache EFProperty from ValueTuple END -->

<!-- ### Commit: 5|🧱: OLTP switch to chassis

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP switch to chassis END -->

<!-- ### Commit: 5|🧱: OLTP DI ScopedRecordsProvider

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI ScopedRecordsProvider END -->

<!-- ### Commit: 5|🧱: OLTP DI ScopedUseCasesProvider

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI ScopedUseCasesProvider END -->

<!-- ### Commit: 5|🧱: OLTP DI inheritance free repo

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI inheritance free repo END -->

<!-- ### Commit: 5|🧱: OLTP record composition to chassis

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP record composition to chassis END -->

<!-- ### Commit: 5|🧱: OLTP DI mixed repository

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI mixed repository END -->

<!-- ### Commit: 5|💾: OLTP EFCore readonly

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 5|💾: OLTP EFCore readonly END -->

<!-- ### Commit: 6|💾: OLTP EFCore finalized

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 6|💾: OLTP EFCore finalized END -->

<!-- ### Commit: 7|🧱: OLTP DI EFCore switch to chassis

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 7|🧱: OLTP DI EFCore switch to chassis END -->

<!-- ### Commit: 7|🧱: OLTP DI EFCore integrated

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 7|🧱: OLTP DI EFCore integrated END -->

<!-- ### Commit: 7|💾: EFCore SortingKey multi-entity

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 7|💾: EFCore SortingKey multi-entity END -->

<!-- ### Commit: 7|💾: EFCore multi-entity sorting

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 7|💾: EFCore multi-entity sorting END -->

<!-- ### Commit: 7|💾: EFCore multi EntityEntry visitors

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: 7|💾: EFCore multi EntityEntry visitors END -->

<!-- ### Commit: N|🧱: OLTP EFCore multi-entity chassis

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: N|🧱: OLTP EFCore multi-entity chassis END -->

<!-- ### Commit: N|🧱: wip switch to chassis

<table><tbody><tr><td>

X files changed, Y insertions(+), Z deletions(-)<br>
<sub><sub>src/YourCompany.Module/</sub></sub><br>
<kbd> +++++++ NNN |⁠ [File.cs                                                                                                        ](src/YourCompany.Configuration/EnvironmentConventions.cs)</kbd><br>

Commit body is multiline and compliant to markdown formatting 💥

</td></tr></tbody></table>

<!-- ### Commit: N|🧱: wip switch to chassis END -->
