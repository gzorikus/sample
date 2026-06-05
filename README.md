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
    mainBranchName: "MERGED"
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
commit type:HIGHLIGHT tag:"7|🧱|public|oltp-di-efcore-handle-specifications-only-the-rest-is-covered|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.DI.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODEI

branch "7|🧱: OLTP DI EFCore switch to chassis" %% ODESTC
%% commit type:HIGHLIGHT tag:"7|🧱|oltp-di-efcore-handle-specifications-only-the-rest-is-covered|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODESTC
commit type:NORMAL tag:"7|🧱|YourCompany.Configuration.EFCore|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% ODESTC

%% Startup Iteration 6

checkout "7|🧱: OLTP DI EFCore switch to chassis" %% OEF
branch "6|💾: OLTP EFCore finalized" %% OEF
commit type:NORMAL tag:"6|💾|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% OEF

%% Startup Iteration 5

checkout "6|💾: OLTP EFCore finalized" %% OER
branch "5|💾: OLTP EFCore readonly" %% OER
%% commit type:REVERSE %% OER>1
%% commit type:REVERSE %% OER>2
%% commit type:REVERSE %% OER>3
%% commit type:REVERSE %% OER>4
%% commit type:REVERSE %% OER>5
commit type:NORMAL tag:"5|💾|YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore" %% OER

%% checkout "7|🧱: OLTP DI EFCore integrated" %% ODMR
%% checkout "N|🧱: OLTP EFCore multi-entity chassis" %% ODMR
%% branch "5|🧱: OLTP DI mixed repository" %% ODMR
%% commit type:REVERSE %% ODMR>1
%% commit type:REVERSE %% ODMR>2
%% commit type:REVERSE %% ODMR>3
%% commit type:REVERSE %% ODMR>4
%% commit type:REVERSE %% ODMR>5
commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition" %% ODMR

branch "5|🧱: OLTP record composition to chassis" %% ORCTC
%% commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% ORCTC
commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% ORCTC

checkout "7|🧱: OLTP DI EFCore switch to chassis" %% ODIFR
branch "5|🧱: OLTP DI inheritance free repo" %% ODIFR
commit type:REVERSE %% ODIFR>1
commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance|YourCompany.OLTP.RecordsManagement.DI" %% ODIFR
%% merge "5|🧱: OLTP record composition to chassis" type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance|YourCompany.OLTP.RecordsManagement.DI" %% ODIFR

branch "5|🧱: OLTP DI ScopedUseCasesProvider" %% ODS
commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI" %% 5|🧱: OLTP DI ScopedUseCasesProvider

checkout "5|🧱: OLTP DI inheritance free repo" %% ODSRP
branch "5|🧱: OLTP DI ScopedRecordsProvider" %% ODSRP
commit type:NORMAL tag:"5|🧱|YourCompany.OLTP.StateOwnership.Reflection.DI" %% ODSRP

checkout "5|🧱: OLTP DI inheritance free repo" %% OSTC
%% commit type:REVERSE %% OSTC<1
%% commit type:REVERSE %% OSTC<2
%% commit type:REVERSE %% OSTC<3
branch "5|🧱: OLTP switch to chassis" %% OSTC
commit type:REVERSE %% OSTC>1
commit type:HIGHLIGHT tag:"5|🧱|oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis|YourCompany.OLTP.RecordsManagement|YourCompany.OLTP.RecordsManagement.DI|YourCompany.OLTP.RecordsManagement.Persistence|YourCompany.OLTP.RecordsManagement.UseCases|YourCompany.OLTP.StateOwnership" %% OSTC

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

checkout "6|💾: OLTP EFCore finalized" %% EETSKT
branch "4|💾: EFEntityTypeSortingKeyTopology" %% EETSKT
commit type:REVERSE %% EETSKT>1
commit type:REVERSE %% EETSKT>2
commit type:REVERSE %% EETSKT>3
commit type:REVERSE %% EETSKT>4
commit type:REVERSE %% EETSKT>5
%% commit type:REVERSE %% EETSKT>6
%% commit type:REVERSE %% EETSKT>7
commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% EETSKT

checkout "5|💾: OLTP EFCore readonly" %% Startup Iteration 4
commit type:REVERSE %% OER 4>1
commit type:REVERSE %% OER 4>2
commit type:REVERSE %% OER 4>3

checkout "5|💾: OLTP EFCore readonly" %% OEIG
branch "4|💾: OLTP EFCore ID generation" %% OEIG
commit type:REVERSE %% OEIG>1
commit type:NORMAL tag:"4|💾|YourCompany.OLTP.StateOwnership.Reflection.EFCore" %% OEIG

branch "4|💾: OLTP EFCore MetadataHelper" %% OEMH
commit type:NORMAL tag:"4|💾|YourCompany.OLTP.StateOwnership.Reflection.EFCore" %% OEMH

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

checkout "5|💾: OLTP EFCore readonly" %% ESKEE
branch "4|💾: EFCore SortingKey ⇆ EntityEntry" %% ESKEE
commit type:REVERSE %% ESKEE>1
commit type:REVERSE %% ESKEE>2
commit type:REVERSE %% ESKEE>3
commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKEE
%% merge "7|💾: EFCore multi EntityEntry visitors" type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKEE

branch "4|💾: EFEntityEntryPropertiesCache" %% EEEPC
commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% EEEPC

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

checkout "5|💾: OLTP EFCore readonly" %% ESKR
branch "4|💾: EFCore SortingKey reading" %% ESKR
commit type:REVERSE %% ESKR>1
commit type:REVERSE %% ESKR>2
commit type:REVERSE %% ESKR>3
commit type:REVERSE %% ESKR>4
commit type:REVERSE %% ESKR>5
commit type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKR
%% merge "7|💾: EFCore multi-entity sorting" type:NORMAL tag:"4|💾|YourCompany.Configuration.EFCore" %% ESKR

checkout "5|🧱: OLTP switch to chassis" %% OLMBU
branch "4|🧱: OLTP LINQ may be useful" %% OLMBU
%% commit type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence.Linq" %% OLMBU
merge "5|💾: OLTP EFCore readonly" type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence.Linq" %% OLMBU

branch "4|🧱: OLTP identities are unique keys" %% OIAUK
commit type:REVERSE %% OIAUK>1
%% commit type:HIGHLIGHT tag:"4|🧱|oltp-segregate-persistence-repository-is-for-use-cases|YourCompany.OLTP.RecordsManagement.Persistence" %% OIAUK
commit type:NORMAL tag:"4|🧱|YourCompany.OLTP.RecordsManagement.Persistence" %% OIAUK

checkout "5|🧱: OLTP record composition to chassis" %% OUCBRT
branch "4|🏠: OLTP use cases by record type" %% OUCBRT
commit type:REVERSE %% OUCBRT>1
commit type:REVERSE %% OUCBRT>2
commit type:REVERSE %% OUCBRT>3
commit type:REVERSE %% OUCBRT>4
commit type:REVERSE %% OUCBRT>5
commit type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-with-transactional-composition|YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition" %% OUCBRT

%% checkout "4|🏠: OLTP use cases by record type" %% EOUC
checkout "5|🧱: OLTP switch to chassis" %% EOUC
branch "4|🏠: enumerate OLTP use cases" %% EOUC
%% commit type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-repository-is-for-persistence|YourCompany.OLTP.RecordsManagement.UseCases" %% EOUC
merge "4|🏠: OLTP use cases by record type" type:HIGHLIGHT tag:"4|🏠|oltp-segregate-use-cases-repository-is-for-persistence|YourCompany.OLTP.RecordsManagement.UseCases" %% EOUC

%% Startup Iteration 3

checkout "4|💾: EFEntityTypeSortingKeyTopology" %% Startup Iteration 3
commit type:REVERSE %% EETSKT 3>1
commit type:REVERSE %% EETSKT 3>2
commit type:REVERSE %% EETSKT 3>3
commit type:REVERSE %% EETSKT 3>4
commit type:REVERSE %% EETSKT 3>5
%% commit type:REVERSE %% EETSKT 3>6

%% checkout "4|💾: EFCore SortingKey ⇆ EntityEntry" %% Startup Iteration 3
%% commit type:REVERSE %% ESKEE 3>1

checkout "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
%% checkout "7|💾: EFCore SortingKey multi-entity" %% EFCSK
branch "3|💾: EFCore first-class SortingKey" %% EFCSK
%% merge "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
merge "4|💾: EFCore SortingKey reading" %% EFCSK
%% merge "4|💾: EFCore SortingKey ⇆ EntityEntry" %% EFCSK
merge "4|💾: EFEntityTypeSortingKeyTopology" %% EFCSK
commit type:REVERSE %% EFCSK>1
%% commit type:REVERSE %% EFCSK>2
%% commit type:REVERSE %% EFCSK>3
%% commit type:REVERSE %% EFCSK>4
commit type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% EFCSK

%% checkout "4|💾: cache EFProperty from ValueTuple" %% Startup Iteration 3
%% commit type:REVERSE %% CEPFVT 3>1
%% commit type:REVERSE %% CEPFVT 3>2
%% commit type:REVERSE %% CEPFVT 3>3
%% commit type:REVERSE %% CEPFVT 3>4
%% commit type:REVERSE %% CEPFVT 3>5
%% commit type:REVERSE %% CEPFVT 3>6
%% commit type:REVERSE %% CEPFVT 3>7

%% checkout "3|💾: EFCore first-class SortingKey" %% EPEC
branch "3|💾: EFPropertyExpressionsCache" %% EPEC
commit type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore" %% EPEC
%% merge "4|💾: cache EFProperty from ValueTuple" type:NORMAL tag:"3|💾|YourCompany.Configuration.EFCore" %% EPEC

%% checkout "4|🧱: OLTP identities are unique keys" %% Startup Iteration 3
%% commit type:REVERSE %% OIAUK 3>1

checkout "4|🏠: enumerate OLTP use cases" %% OTS
%% checkout "4|🧱: OLTP identities are unique keys" %% OTS
branch "3|🧱: OLTP transaction structure" %% OTS
merge "4|🧱: OLTP identities are unique keys" %% OTS
commit type:REVERSE %% OTS>1
commit type:REVERSE %% OTS>2
commit type:REVERSE %% OTS>3
commit type:REVERSE %% OTS>4
commit type:REVERSE %% OTS>5
commit type:REVERSE %% OTS>6
%% commit type:REVERSE %% OTS>7
commit type:HIGHLIGHT tag:"3|🧱|oltp-typical-transaction-structure-repository-is-up-to-you|YourCompany.OLTP.RecordsManagement" %% OTS
%% merge "4|🧱: OLTP identities are unique keys" type:HIGHLIGHT tag:"3|🧱|oltp-typical-transaction-structure-repository-is-up-to-you|YourCompany.OLTP.RecordsManagement" %% OTS

checkout "4|🏠: OLTP use cases by record type" %% ORTCM
branch "3|🏠: OLTP RecordTypesCompositionMap" %% ORTCM
commit type:REVERSE %% ORTCM>1
commit type:REVERSE %% ORTCM>2
%% commit type:REVERSE %% ORTCM>3
%% commit type:REVERSE %% ORTCM>4
%% commit type:REVERSE %% ORTCM>5
%% commit type:REVERSE %% ORTCM>6
%% commit type:REVERSE %% ORTCM>7
%% commit type:REVERSE %% ORTCM>8
%% commit type:REVERSE %% ORTCM>9
%% commit type:REVERSE %% ORTCM>10
commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection" %% ORTCM

checkout "5|🧱: OLTP DI ScopedUseCasesProvider" %% OUCTM
commit type:REVERSE %% OUCTM<1
commit type:REVERSE %% OUCTM<2
commit type:REVERSE %% OUCTM<3
commit type:REVERSE %% OUCTM<4
commit type:REVERSE %% OUCTM<5
commit type:REVERSE %% OUCTM<6
commit type:REVERSE %% OUCTM<7
commit type:REVERSE %% OUCTM<8
commit type:REVERSE %% OUCTM<9
commit type:REVERSE %% OUCTM<10
commit type:REVERSE %% OUCTM<11
%% commit type:REVERSE %% OUCTM<12
%% commit type:REVERSE %% OUCTM<13
%% commit type:REVERSE %% OUCTM<14
%% commit type:REVERSE %% OUCTM<15
branch "3|🏠: OLTP UseCaseTypesMap" %% OUCTM
%% commit type:REVERSE %% OUCTM>1
%% commit type:REVERSE %% OUCTM>2
%% commit type:REVERSE %% OUCTM>3
%% commit type:REVERSE %% OUCTM>4
commit type:REVERSE %% OUCTM>5
commit type:REVERSE %% OUCTM>6
commit type:REVERSE %% OUCTM>7
commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases.Reflection" %% OUCTM

checkout "4|🏠: enumerate OLTP use cases" %% GOUC
branch "3|🏠: generic OLTP use cases" %% GOUC
%% commit type:REVERSE %% GOUC>1
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases" %% GOUC
merge "3|🏠: OLTP UseCaseTypesMap" type:NORMAL tag:"3|🏠|YourCompany.OLTP.RecordsManagement.UseCases" %% GOUC

checkout "4|💾: OLTP EFCore MetadataHelper" %% Startup Iteration 3
commit type:REVERSE %% OEMH 3>1
commit type:REVERSE %% OEMH 3>2
commit type:REVERSE %% OEMH 3>3
commit type:REVERSE %% OEMH 3>4
commit type:REVERSE %% OEMH 3>5
commit type:REVERSE %% OEMH 3>6
commit type:REVERSE %% OEMH 3>7
commit type:REVERSE %% OEMH 3>8
commit type:REVERSE %% OEMH 3>9
commit type:REVERSE %% OEMH 3>10
%% commit type:REVERSE %% OEMH 3>11
%% commit type:REVERSE %% OEMH 3>12
%% commit type:REVERSE %% OEMH 3>13
%% commit type:REVERSE %% OEMH 3>14

%% checkout "3|🏠: OLTP RecordTypesCompositionMap" %% ORTM
checkout "5|🧱: OLTP DI ScopedRecordsProvider" %% ORTM
branch "3|🏠: OLTP RecordTypesMap" %% ORTM
%% merge "3|🏠: OLTP RecordTypesCompositionMap" %% ORTM
%% commit type:REVERSE %% ORTM>1
%% commit type:REVERSE %% ORTM>2
%% commit type:REVERSE %% ORTM>3
%% commit type:REVERSE %% ORTM>4
%% commit type:REVERSE %% ORTM>5
%% commit type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM
merge "3|🏠: OLTP RecordTypesCompositionMap" type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM
merge "4|💾: OLTP EFCore MetadataHelper" type:NORMAL tag:"3|🏠|YourCompany.OLTP.StateOwnership.Reflection" %% ORTM

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

checkout "6|💾: OLTP EFCore finalized" %% ECTTGS
commit type:REVERSE %% ECTTGS<1
commit type:REVERSE %% ECTTGS<2
commit type:REVERSE %% ECTTGS<3
commit type:REVERSE %% ECTTGS<4
commit type:REVERSE %% ECTTGS<5
commit type:REVERSE %% ECTTGS<6
commit type:REVERSE %% ECTTGS<7
commit type:REVERSE %% ECTTGS<8
commit type:REVERSE %% ECTTGS<9
commit type:REVERSE %% ECTTGS<10
commit type:REVERSE %% ECTTGS<11
commit type:REVERSE %% ECTTGS<12
commit type:REVERSE %% ECTTGS<13
commit type:REVERSE %% ECTTGS<14
commit type:REVERSE %% ECTTGS<15
commit type:REVERSE %% ECTTGS<16
commit type:REVERSE %% ECTTGS<17
commit type:REVERSE %% ECTTGS<18
commit type:REVERSE %% ECTTGS<19
%% commit type:REVERSE %% ECTTGS<20
%% commit type:REVERSE %% ECTTGS<21
%% commit type:REVERSE %% ECTTGS<22
%% commit type:REVERSE %% ECTTGS<23
%% commit type:REVERSE %% ECTTGS<24
%% commit type:REVERSE %% ECTTGS<25
branch "2|💾: EFChangeTrackerTrackGraphStrategy" %% ECTTGS
commit type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore" %% ECTTGS

checkout "6|💾: OLTP EFCore finalized" %% EPL
branch "2|💾: EFCore pessimistic locking" %% EPL
commit type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% EPL

checkout "4|💾: EFEntityEntryPropertiesCache" %% BPE
branch "2|💾: basic pluggable EFCore" %% BPE

checkout "2|💾: EFChangeTrackerTrackGraphStrategy" %% BPE
commit type:REVERSE %% ECTTGS BPE>1

checkout "2|💾: basic pluggable EFCore" %% BPE
merge "3|💾: EFPropertyExpressionsCache" %% BPE
merge "2|💾: EFCore pessimistic locking" %% BPE
%% merge "2|💾: EFChangeTrackerTrackGraphStrategy" %% BPE
merge "2|💾: EFChangeTrackerTrackGraphStrategy" type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% BPE
%% merge "2|💾: EFCore hosting migration run" type:NORMAL tag:"2|💾|YourCompany.Configuration.EFCore|YourCompany.Configuration.EFCore.PostgreSQL|YourCompany.Configuration.EFCore.Sqlite" %% BPE

checkout "3|🏠: OLTP RecordTypesCompositionMap" %% TCM
branch "2|🧱: TypesCompositionMap" %% TCM
commit type:REVERSE %% TCM>1
commit type:REVERSE %% TCM>2
commit type:REVERSE %% TCM>3
%% commit type:REVERSE %% TCM>4
%% commit type:REVERSE %% TCM>5
%% commit type:REVERSE %% TCM>6
%% commit type:REVERSE %% TCM>7
%% commit type:REVERSE %% TCM>8
commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TCM

checkout "3|🏠: OLTP UseCaseTypesMap" %% TAH
%% commit type:REVERSE %% TAH<1
%% commit type:REVERSE %% TAH<2
branch "2|🧱: TypeAbstractionsHelper" %% TAH
commit type:REVERSE %% TAH>1
commit type:REVERSE %% TAH>2
commit type:REVERSE %% TAH>3
commit type:REVERSE %% TAH>4
commit type:REVERSE %% TAH>5
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TAH
merge "2|🧱: TypesCompositionMap" type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% TAH

checkout "4|💾: EFCore SortingKey reading" %% VTH
%% commit type:REVERSE %% VTH<1
%% commit type:REVERSE %% VTH<2
%% commit type:REVERSE %% VTH<3
%% commit type:REVERSE %% VTH<4
branch "2|🧱: ValueTupleHelper" %% VTH
%% merge "4|💾: cache EFProperty from ValueTuple" %% VTH
commit type:REVERSE %% VTH>1
commit type:REVERSE %% VTH>2
commit type:REVERSE %% VTH>3
commit type:REVERSE %% VTH>4
commit type:REVERSE %% VTH>5
commit type:REVERSE %% VTH>6
commit type:REVERSE %% VTH>7
commit type:REVERSE %% VTH>8
commit type:REVERSE %% VTH>9
commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% VTH

checkout "5|🧱: OLTP DI ScopedRecordsProvider" %% DCH
branch "2|🧱: DIConstructorHelper" %% DCH
commit type:REVERSE %% DCH>1
commit type:REVERSE %% DCH>2
commit type:REVERSE %% DCH>3
commit type:REVERSE %% DCH>4
commit type:REVERSE %% DCH>5
commit type:REVERSE %% DCH>6
commit type:REVERSE %% DCH>7
commit type:REVERSE %% DCH>8
commit type:REVERSE %% DCH>9
commit type:REVERSE %% DCH>10
commit type:REVERSE %% DCH>11
commit type:REVERSE %% DCH>12
commit type:REVERSE %% DCH>13
commit type:REVERSE %% DCH>14
commit type:REVERSE %% DCH>15
commit type:REVERSE %% DCH>16
commit type:REVERSE %% DCH>17
%% commit type:REVERSE %% DCH>18
%% commit type:REVERSE %% DCH>19
%% commit type:REVERSE %% DCH>20
%% commit type:REVERSE %% DCH>21
%% commit type:REVERSE %% DCH>22
commit type:NORMAL tag:"2|🧱|YourCompany.Reflection.DI" %% DCH

checkout "3|💾: EFPropertyExpressionsCache" %% Startup Iteration 2
commit type:REVERSE %% EPEC 2>1

%% checkout "2|🧱: TypeAbstractionsHelper" %% GMH
checkout "4|💾: EFEntityEntryPropertiesCache" %% GMH
branch "2|🧱: GetMemberHelper" %% GMH
merge "3|💾: EFPropertyExpressionsCache" %% GMH
%% commit type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% GMH
merge "2|🧱: DIConstructorHelper" type:NORMAL tag:"2|🧱|YourCompany.Reflection" %% GMH

checkout "3|🏠: generic OLTP use cases" %% ATL
branch "2|🧱: AwaitTasksList" %% ATL
commit type:REVERSE %% ATL>1
%% commit type:REVERSE %% ATL>2
commit type:REVERSE %% ATL>3
commit type:REVERSE %% ATL>4
commit type:REVERSE %% ATL>5
commit type:NORMAL tag:"2|🧱|YourCompany.Threading" %% ATL

checkout "4|🧱: OLTP identities are unique keys" %% SH
branch "2|🧱: StructHelper" %% SH
commit type:REVERSE %% SH>1
commit type:REVERSE %% SH>2
commit type:REVERSE %% SH>3
commit type:REVERSE %% SH>4
commit type:REVERSE %% SH>5
commit type:REVERSE %% SH>6
commit type:REVERSE %% SH>7
commit type:REVERSE %% SH>8
commit type:REVERSE %% SH>9
commit type:REVERSE %% SH>10
commit type:REVERSE %% SH>11
commit type:REVERSE %% SH>12
commit type:REVERSE %% SH>13
commit type:REVERSE %% SH>14
%% commit type:REVERSE %% SH>15
%% commit type:REVERSE %% SH>16
%% commit type:REVERSE %% SH>17
%% commit type:REVERSE %% SH>18
commit type:NORMAL tag:"2|🧱|YourCompany.CompilerServices" %% SH

checkout "3|🧱: OLTP transaction structure" %% Startup Iteration 2
commit type:REVERSE %% OTS 2>1
commit type:REVERSE %% OTS 2>2
commit type:REVERSE %% OTS 2>3
commit type:REVERSE %% OTS 2>4
commit type:REVERSE %% OTS 2>5
commit type:REVERSE %% OTS 2>6
commit type:REVERSE %% OTS 2>7
commit type:REVERSE %% OTS 2>8
%% commit type:REVERSE %% OTS 2>9
%% commit type:REVERSE %% OTS 2>10
%% commit type:REVERSE %% OTS 2>11
%% commit type:REVERSE %% OTS 2>12

checkout "3|🏠: generic OLTP use cases" %% OTILS
%% checkout "3|🧱: OLTP transaction structure" %% OTILS
branch "2|🏠: OLTP transaction is limited size" %% OTILS
%% commit type:HIGHLIGHT tag:"2|🏠|oltp-ways-to-access-size-limited-record-batch|YourCompany.OLTP.RecordsManagement" %% OTILS
merge "3|🧱: OLTP transaction structure" type:HIGHLIGHT tag:"2|🏠|oltp-ways-to-access-size-limited-record-batch|YourCompany.OLTP.RecordsManagement" %% OTILS

%% Startup Iteration 1

%% checkout "MERGED" %% BCC
checkout "2|💾: basic pluggable EFCore" %% BCC
branch "1|🧱: basic configurations covered" %% BCC
commit type:REVERSE %% BCC>1
%% commit type:REVERSE %% BCC>2
%% commit type:REVERSE %% BCC>3
%% commit type:REVERSE %% BCC>4
commit type:HIGHLIGHT tag:"1|🧱|configuration-entry-assembly-rotating-secrets-scaling-plugins|YourCompany.Configuration" %% BCC

checkout "3|🏠: OLTP RecordTypesCompositionMap" %% OTC
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
branch "1|🏠: OLTP transactional composition" %% OTC
commit type:REVERSE %% OTC>1
commit type:REVERSE %% OTC>2
commit type:REVERSE %% OTC>3
commit type:REVERSE %% OTC>4
commit type:REVERSE %% OTC>5
commit type:REVERSE %% OTC>6
%% commit type:REVERSE %% OTC>7
%% commit type:REVERSE %% OTC>8
%% commit type:REVERSE %% OTC>9
%% commit type:REVERSE %% OTC>10
%% commit type:REVERSE %% OTC>11
%% commit type:REVERSE %% OTC>12
%% commit type:REVERSE %% OTC>13
%% commit type:REVERSE %% OTC>14
commit type:NORMAL tag:"1|🏠|YourCompany.OLTP.StateOwnership.TransactionalComposition" %% OTC

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

checkout "2|🏠: OLTP transaction is limited size" %% Startup Iteration 1
commit type:REVERSE %% OTILS 1>1
%% commit type:REVERSE %% OTILS 1>2
%% commit type:REVERSE %% OTILS 1>3

%% checkout MERGED %% THOOIT
checkout "2|🏠: OLTP transaction is limited size" %% THOOIT
checkout "3|🏠: OLTP RecordTypesMap" %% THOOIT
branch "1|🏠: the heart of OLTP is transaction" %% THOOIT
%% merge "2|🏠: OLTP transaction is limited size" %% THOOIT
%% merge "1|🏠: OLTP events producing" %% THOOIT
merge "1|🏠: OLTP transactional composition" %% THOOIT
%% merge "N|🧱: wip switch to chassis" %% THOOIT
%% commit type:HIGHLIGHT tag:"1|🏠|oltp-basic-object-model-the-rest-is-up-to-you|YourCompany.OLTP.StateOwnership" %% THOOIT
merge "2|🏠: OLTP transaction is limited size" type:HIGHLIGHT tag:"1|🏠|oltp-basic-object-model-the-rest-is-up-to-you|YourCompany.OLTP.StateOwnership" %% THOOIT
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

### Branch: oltp-di-efcore-handle-specifications-only-the-rest-is-covered

> Files: 203 | Lines: 17310  
Pros: out-of-the-box full-fledged persistence utilizing the linq record data builders  
Cons: significant entry point complexity increase because of covering EFCore missing capabilities

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [1\|⁠🧱: basic configurations covered](#commit-1-basic-configurations-covered) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-basic-configurations-covered) </kbd> | <kbd>YourCompany.Configuration (+637 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: StructHelper](#commit-2-structhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-structhelper) </kbd> | <kbd>YourCompany.CompilerServices (+18 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [2\|⁠🧱: GetMemberHelper](#commit-2-getmemberhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-getmemberhelper) </kbd> | <kbd>YourCompany.Reflection (+57 lines)</kbd> |
| [2\|⁠🧱: DIConstructorHelper](#commit-2-diconstructorhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-diconstructorhelper) </kbd> | <kbd>YourCompany.Reflection.DI (+31 lines)</kbd> |
| [2\|⁠🧱: ValueTupleHelper](#commit-2-valuetuplehelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-valuetuplehelper) </kbd> | <kbd>YourCompany.Reflection (+143 lines)</kbd> |
| [2\|⁠🧱: TypeAbstractionsHelper](#commit-2-typeabstractionshelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typeabstractionshelper) </kbd> | <kbd>YourCompany.Reflection (+26 lines)</kbd> |
| [2\|⁠💾: basic pluggable EFCore](#commit-2-basic-pluggable-efcore) | <kbd> [efcore‑migrations](examples/efcore-migrations/README.md#commit-2-basic-pluggable-efcore) </kbd> | <kbd>YourCompany.Configuration.EFCore (+429 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.PostgreSQL (+181 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.Sqlite (+71 lines)</kbd> |
| [2\|⁠💾: EFCore pessimistic locking](#commit-2-efcore-pessimistic-locking) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-efcore-pessimistic-locking) </kbd> | <kbd>YourCompany.Configuration.EFCore (+222 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.PostgreSQL (+59 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.Sqlite (+47 lines)</kbd> |
| [2\|⁠💾: EFChangeTrackerTrackGraphStrategy](#commit-2-efchangetrackertrackgraphstrategy) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-efchangetrackertrackgraphstrategy) </kbd> | <kbd>YourCompany.Configuration.EFCore (+106 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesMap](#commit-3-oltp-recordtypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypesmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection (+263 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🏠: OLTP UseCaseTypesMap](#commit-3-oltp-usecasetypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-usecasetypesmap) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection (+176 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [3\|⁠💾: EFPropertyExpressionsCache](#commit-3-efpropertyexpressionscache) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-efpropertyexpressionscache) </kbd> | <kbd>YourCompany.Configuration.EFCore (+87 lines)</kbd> |
| [3\|⁠💾: EFCore first-class SortingKey](#commit-3-efcore-first-class-sortingkey) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-efcore-first-class-sortingkey) </kbd> | <kbd>YourCompany.Configuration.EFCore (+2388 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.PostgreSQL (+37 lines)</kbd><br><kbd>YourCompany.Configuration.EFCore.Sqlite (+37 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |
| [4\|⁠🧱: OLTP identities are unique keys](#commit-4-oltp-identities-are-unique-keys) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-identities-are-unique-keys) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence (+1642 lines)</kbd> |
| [4\|⁠🧱: OLTP LINQ may be useful](#commit-4-oltp-linq-may-be-useful) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-linq-may-be-useful) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq (+59 lines)</kbd> |
| [4\|⁠💾: EFCore SortingKey reading](#commit-4-efcore-sortingkey-reading) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-efcore-sortingkey-reading) </kbd> | <kbd>YourCompany.Configuration.EFCore (+789 lines)</kbd> |
| [4\|⁠💾: EFEntityEntryPropertiesCache](#commit-4-efentityentrypropertiescache) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-efentityentrypropertiescache) </kbd> | <kbd>YourCompany.Configuration.EFCore (+63 lines)</kbd> |
| [4\|⁠💾: EFCore SortingKey ⇆ EntityEntry](#commit-4-efcore-sortingkey-entityentry) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-efcore-sortingkey-entityentry) </kbd> | <kbd>YourCompany.Configuration.EFCore (+763 lines)</kbd> |
| [4\|⁠💾: OLTP EFCore MetadataHelper](#commit-4-oltp-efcore-metadatahelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-efcore-metadatahelper) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection.EFCore (+45 lines)</kbd> |
| [4\|⁠💾: OLTP EFCore ID generation](#commit-4-oltp-efcore-id-generation) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-efcore-id-generation) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection.EFCore (+111 lines)</kbd> |
| [4\|⁠💾: EFEntityTypeSortingKeyTopology](#commit-4-efentitytypesortingkeytopology) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-efentitytypesortingkeytopology) </kbd> | <kbd>YourCompany.Configuration.EFCore (+347 lines)</kbd> |
| [5\|⁠🧱: OLTP switch to chassis](#commit-5-oltp-switch-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-switch-to-chassis) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.DI (+122 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence (-26 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.UseCases (-117 lines)</kbd><br><kbd>YourCompany.OLTP.StateOwnership (+6 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedRecordsProvider](#commit-5-oltp-di-scopedrecordsprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedrecordsprovider) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection.DI (+120 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedUseCasesProvider](#commit-5-oltp-di-scopedusecasesprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedusecasesprovider) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI (+69 lines)</kbd> |
| [5\|⁠🧱: OLTP DI inheritance free repo](#commit-5-oltp-di-inheritance-free-repo) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-inheritance-free-repo) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.DI (+444 lines)</kbd> |
| [5\|⁠💾: OLTP EFCore readonly](#commit-5-oltp-efcore-readonly) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-efcore-readonly) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore (+2256 lines)</kbd> |
| [6\|⁠💾: OLTP EFCore finalized](#commit-6-oltp-efcore-finalized) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-6-oltp-efcore-finalized) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore (+1108 lines)</kbd> |
| [7\|⁠🧱: OLTP DI EFCore switch to chassis](#commit-7-oltp-di-efcore-switch-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-7-oltp-di-efcore-switch-to-chassis) </kbd> | <kbd>YourCompany.Configuration.EFCore (+4 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore (+1 lines)</kbd> |
| [7\|⁠🧱: OLTP DI EFCore integrated](#commit-7-oltp-di-efcore-integrated) | <kbd> [efcore‑migrations](examples/efcore-migrations/README.md#commit-7-oltp-di-efcore-integrated) </kbd> | <kbd>YourCompany.Configuration.EFCore (0 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.DI.EFCore (+319 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore (+2 lines)</kbd> |

<!-- ### Branch: oltp-di-efcore-handle-specifications-only-the-rest-is-covered END -->

### Branch: oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition

> Files: 119 | Lines: 9681  
Pros: no boilerplate required for transactional composition, domain modularity achieved  
Cons: still lacking major things like events producing, persistence

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [1\|⁠🏠: OLTP transactional composition](#commit-1-oltp-transactional-composition) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-oltp-transactional-composition) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.TransactionalComposition (+20 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: StructHelper](#commit-2-structhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-structhelper) </kbd> | <kbd>YourCompany.CompilerServices (+18 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [2\|⁠🧱: GetMemberHelper](#commit-2-getmemberhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-getmemberhelper) </kbd> | <kbd>YourCompany.Reflection (+57 lines)</kbd> |
| [2\|⁠🧱: DIConstructorHelper](#commit-2-diconstructorhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-diconstructorhelper) </kbd> | <kbd>YourCompany.Reflection.DI (+31 lines)</kbd> |
| [2\|⁠🧱: TypeAbstractionsHelper](#commit-2-typeabstractionshelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typeabstractionshelper) </kbd> | <kbd>YourCompany.Reflection (+26 lines)</kbd> |
| [2\|⁠🧱: TypesCompositionMap](#commit-2-typescompositionmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typescompositionmap) </kbd> | <kbd>YourCompany.Reflection (+1050 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesMap](#commit-3-oltp-recordtypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypesmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection (+263 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🏠: OLTP UseCaseTypesMap](#commit-3-oltp-usecasetypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-usecasetypesmap) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection (+176 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesCompositionMap](#commit-3-oltp-recordtypescompositionmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypescompositionmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection (+260 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |
| [4\|⁠🏠: OLTP use cases by record type](#commit-4-oltp-use-cases-by-record-type) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-use-cases-by-record-type) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition (+770 lines)</kbd> |
| [4\|⁠🧱: OLTP identities are unique keys](#commit-4-oltp-identities-are-unique-keys) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-identities-are-unique-keys) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence (+1642 lines)</kbd> |
| [4\|⁠🧱: OLTP LINQ may be useful](#commit-4-oltp-linq-may-be-useful) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-linq-may-be-useful) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq (+59 lines)</kbd> |
| [5\|⁠🧱: OLTP switch to chassis](#commit-5-oltp-switch-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-switch-to-chassis) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.DI (+122 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence (-26 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.UseCases (-117 lines)</kbd><br><kbd>YourCompany.OLTP.StateOwnership (+6 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedRecordsProvider](#commit-5-oltp-di-scopedrecordsprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedrecordsprovider) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection.DI (+120 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedUseCasesProvider](#commit-5-oltp-di-scopedusecasesprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedusecasesprovider) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI (+69 lines)</kbd> |
| [5\|⁠🧱: OLTP DI inheritance free repo](#commit-5-oltp-di-inheritance-free-repo) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-inheritance-free-repo) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.DI (+444 lines)</kbd> |
| [5\|⁠🧱: OLTP record composition to chassis](#commit-5-oltp-record-composition-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-record-composition-to-chassis) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition (+777 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition (-770 lines)</kbd><br><kbd>YourCompany.OLTP.StateOwnership.TransactionalComposition (0 lines)</kbd> |
| [5\|⁠🧱: OLTP DI mixed repository](#commit-5-oltp-di-mixed-repository) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-mixed-repository) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition (+516 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-with-record-types-composition END -->

### Branch: oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance

> Files: 82 | Lines: 7058  
Pros: avoid inheriting per entity, contravariant use cases auto-injection, good starting point for adapters  
Cons: still lacking major things like events producing, transactional composition (domain modularity), persistence

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: StructHelper](#commit-2-structhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-structhelper) </kbd> | <kbd>YourCompany.CompilerServices (+18 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [2\|⁠🧱: GetMemberHelper](#commit-2-getmemberhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-getmemberhelper) </kbd> | <kbd>YourCompany.Reflection (+57 lines)</kbd> |
| [2\|⁠🧱: DIConstructorHelper](#commit-2-diconstructorhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-diconstructorhelper) </kbd> | <kbd>YourCompany.Reflection.DI (+31 lines)</kbd> |
| [2\|⁠🧱: TypeAbstractionsHelper](#commit-2-typeabstractionshelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typeabstractionshelper) </kbd> | <kbd>YourCompany.Reflection (+26 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesMap](#commit-3-oltp-recordtypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypesmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection (+263 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🏠: OLTP UseCaseTypesMap](#commit-3-oltp-usecasetypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-usecasetypesmap) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection (+176 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |
| [4\|⁠🧱: OLTP identities are unique keys](#commit-4-oltp-identities-are-unique-keys) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-identities-are-unique-keys) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence (+1642 lines)</kbd> |
| [4\|⁠🧱: OLTP LINQ may be useful](#commit-4-oltp-linq-may-be-useful) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-linq-may-be-useful) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq (+59 lines)</kbd> |
| [5\|⁠🧱: OLTP switch to chassis](#commit-5-oltp-switch-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-switch-to-chassis) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.DI (+122 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence (-26 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.UseCases (-117 lines)</kbd><br><kbd>YourCompany.OLTP.StateOwnership (+6 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedRecordsProvider](#commit-5-oltp-di-scopedrecordsprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedrecordsprovider) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection.DI (+120 lines)</kbd> |
| [5\|⁠🧱: OLTP DI ScopedUseCasesProvider](#commit-5-oltp-di-scopedusecasesprovider) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-scopedusecasesprovider) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI (+69 lines)</kbd> |
| [5\|⁠🧱: OLTP DI inheritance free repo](#commit-5-oltp-di-inheritance-free-repo) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-di-inheritance-free-repo) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.DI (+444 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-without-inheritance END -->

### Branch: oltp-segregate-use-cases-with-transactional-composition

> Files: 82 | Lines: 6556  
Pros: clean model with aggregates decomposed by subject/feature effectively grouping use case logic and data  
Cons: a lot of complex boilerplate is produced for manual composition, the map is not as useful as it could be

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [1\|⁠🏠: OLTP transactional composition](#commit-1-oltp-transactional-composition) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-oltp-transactional-composition) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.TransactionalComposition (+20 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [2\|⁠🧱: TypeAbstractionsHelper](#commit-2-typeabstractionshelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typeabstractionshelper) </kbd> | <kbd>YourCompany.Reflection (+26 lines)</kbd> |
| [2\|⁠🧱: TypesCompositionMap](#commit-2-typescompositionmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-typescompositionmap) </kbd> | <kbd>YourCompany.Reflection (+1050 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesMap](#commit-3-oltp-recordtypesmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypesmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.Reflection (+263 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🏠: OLTP RecordTypesCompositionMap](#commit-3-oltp-recordtypescompositionmap) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-recordtypescompositionmap) </kbd> | <kbd>YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection (+260 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |
| [4\|⁠🏠: OLTP use cases by record type](#commit-4-oltp-use-cases-by-record-type) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-use-cases-by-record-type) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition (+770 lines)</kbd> |

<!-- ### Branch: oltp-segregate-use-cases-with-transactional-composition END -->

### Branch: oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis

> Files: 61 | Lines: 5872  
Pros: reduced public surface and boilerplate, no all-in-one inheriting, jobs segregated, maintain your chassis separately  
Cons: still requires repository per entity, lacks DI, events, transactional composition (domain modularity), persistence

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: StructHelper](#commit-2-structhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-structhelper) </kbd> | <kbd>YourCompany.CompilerServices (+18 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |
| [4\|⁠🧱: OLTP identities are unique keys](#commit-4-oltp-identities-are-unique-keys) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-identities-are-unique-keys) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence (+1642 lines)</kbd> |
| [4\|⁠🧱: OLTP LINQ may be useful](#commit-4-oltp-linq-may-be-useful) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-linq-may-be-useful) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq (+59 lines)</kbd> |
| [5\|⁠🧱: OLTP switch to chassis](#commit-5-oltp-switch-to-chassis) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-5-oltp-switch-to-chassis) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.DI (+122 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.Persistence (-26 lines)</kbd><br><kbd>YourCompany.OLTP.RecordsManagement.UseCases (-117 lines)</kbd><br><kbd>YourCompany.OLTP.StateOwnership (+6 lines)</kbd> |

<!-- ### Branch: oltp-di-combine-use-case-and-persistence-interfaces-switch-to-chassis END -->

### Branch: oltp-segregate-persistence-repository-is-for-use-cases

> Files: 56 | Lines: 5508  
Pros: clarified identities and streamlined persistence implementation  
Cons: may be not enough organized if you need to reuse some use cases

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: StructHelper](#commit-2-structhelper) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-structhelper) </kbd> | <kbd>YourCompany.CompilerServices (+18 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🧱: OLTP identities are unique keys](#commit-4-oltp-identities-are-unique-keys) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-identities-are-unique-keys) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence (+1642 lines)</kbd> |
| [4\|⁠🧱: OLTP LINQ may be useful](#commit-4-oltp-linq-may-be-useful) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-oltp-linq-may-be-useful) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.Persistence.Linq (+59 lines)</kbd> |

<!-- ### Branch: oltp-segregate-persistence-repository-is-for-use-cases END -->

### Branch: oltp-segregate-use-cases-repository-is-for-persistence

> Files: 47 | Lines: 4167  
Pros: reuse authorizers, externalize records batch configuration, customize state access  
Cons: still a lot of methods for persistence and complex records batch state

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [2\|⁠🧱: AwaitTasksList](#commit-2-awaittaskslist) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-awaittaskslist) </kbd> | <kbd>YourCompany.Threading (+49 lines)</kbd> |
| [3\|⁠🏠: generic OLTP use cases](#commit-3-generic-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-generic-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+212 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |
| [4\|⁠🏠: enumerate OLTP use cases](#commit-4-enumerate-oltp-use-cases) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-4-enumerate-oltp-use-cases) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement.UseCases (+117 lines)</kbd> |

<!-- ### Branch: oltp-segregate-use-cases-repository-is-for-persistence END -->

### Branch: oltp-typical-transaction-structure-repository-is-up-to-you

> Files: 42 | Lines: 3789  
Pros: have all control structured in one place  
Cons: mixes use case related logic with persistence

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |
| [3\|⁠🧱: OLTP transaction structure](#commit-3-oltp-transaction-structure) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-3-oltp-transaction-structure) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1918 lines)</kbd> |

<!-- ### Branch: oltp-typical-transaction-structure-repository-is-up-to-you END -->

### Branch: oltp-ways-to-access-size-limited-record-batch

> Files: 26 | Lines: 1871  
Pros: enough to standardize use cases or "service 🙈" layer  
Cons: repositories implemented per record type are too big

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |
| [2\|⁠🏠: OLTP transaction is limited size](#commit-2-oltp-transaction-is-limited-size) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-2-oltp-transaction-is-limited-size) </kbd> | <kbd>YourCompany.OLTP.RecordsManagement (+1500 lines)</kbd> |

<!-- ### Branch: oltp-ways-to-access-size-limited-record-batch END -->

### Branch: configuration-entry-assembly-rotating-secrets-scaling-plugins

> Files: 10 | Lines: 637  
Pros: not tied to typical component types, the most common  
Cons: you may find another set of conventions more suitable

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🧱: basic configurations covered](#commit-1-basic-configurations-covered) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-basic-configurations-covered) </kbd> | <kbd>YourCompany.Configuration (+637 lines)</kbd> |

<!-- ### Branch: configuration-entry-assembly-rotating-secrets-scaling-plugins END -->

### Branch: oltp-basic-object-model-the-rest-is-up-to-you

> Files: 6 | Lines: 371  
Pros: isolate your model from anything unrelated to your domain  
Cons: too much boilerplate code being copied for each use case

| Log | Examples | Modules |
|-|-|-|
| [1\|⁠🏠: the heart of OLTP is transaction](#commit-1-the-heart-of-oltp-is-transaction) | <kbd> [TODO](examples/at-least-one-example-solution-per-commit/README.md#commit-1-the-heart-of-oltp-is-transaction) </kbd> | <kbd>YourCompany.OLTP.StateOwnership (+371 lines)</kbd> |

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

### Commit: 1|🏠: the heart of OLTP is transaction

<table><tbody><tr><td>

6 files changed, 371 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership/</sub></sub><br>
<kbd> +++++++ 177 |⁠ [Identity.cs                                                                                                    ](src/YourCompany.OLTP.StateOwnership/Identity.cs)</kbd><br>
<kbd>    ++++ 109 |⁠ [IState.cs                                                                                                      ](src/YourCompany.OLTP.StateOwnership/IState.cs)</kbd><br>
<kbd>      ++ 54  |⁠ [TransactionCallback.cs                                                                                         ](src/YourCompany.OLTP.StateOwnership/TransactionCallback.cs)</kbd><br>
<kbd>       + 14  |⁠ [IStateModifying.cs                                                                                             ](src/YourCompany.OLTP.StateOwnership/IStateModifying.cs)</kbd><br>
<kbd>       + 10  |⁠ [IStateAccess.cs                                                                                                ](src/YourCompany.OLTP.StateOwnership/IStateAccess.cs)</kbd><br>
<kbd>       + 7   |⁠ [ISpecification.cs                                                                                              ](src/YourCompany.OLTP.StateOwnership/ISpecification.cs)</kbd><br>

One atomic action. Heard about DDD "Aggregates"? These are classes  
that represent one transaction per object lifecycle.

Typical transactions modeled consist of one or many modifying actions.  
While overused in practice read-only actions is an edge case (giving  
no relevancy guarantees of the data once accessed).

The mistake № 1 is to model aggregates over abilities and limitations  
of an ORM framework directly. To solve this we're going to delegate  
transactional guarantees from a model (or "services" 🙈) to a  
dedicated state access interface. Respectively the model itself  
abstracts its record data into a separate interface in a manner we're  
going to support (that's why we say we own the framework).

The model accepts its record data's `IStateAccess` via constructor and  
claims its `IState` either read-only or by providing a callback used  
later to assert the modeled data transition from one state to another.  
Once the access is complete the result is placed to `DataAfterAccess`.

In transition `IStateModifying` provides extra data representations:

1. `SettingDataProperties` - represents properties to replace  
persisted values with new non-default-only values (and skip the rest).  
When the aggregate exposes a corresponding record data property  
defined with a setter, the latter sets the value precisely into the  
record data instance behind this state property.  
    > this simplest form of mapping replaces the need for numbers of  
    ~~DTOs-per-"command"~~ as well as for redundant contracts with  
    persistence layer for majority of cases but comes at a cost of the  
    record data's interface to be mutable (defining property setters).  
    With this approach you ensure encapsulation by either means of  
    InternalsVisibleTo or by adding init only checks. Otherwise this  
    state property can be omitted.
2. `LockedRecordData` - provides access to the unchanged record data  
state with relevancy guarantee, i.e. remaining locked in the database.  
During the first callback it is always `null` letting to assert only  
replacing values and/or modifying specifications before accessing DB  
(💡 you can inherit specifications from `EventArgs` to decompose this  
assertion logic with extra callbacks). Once the data is locked against  
other transactions there will be one or more callbacks with  
`LockedRecordData` **not** `null` denoting the only legal and  
completely safe moment to assert **existing data invariants** 😲  
At last it becomes `null` again in the last and only callback,  
denoting the end of data access by this aggregate instance.

As mentioned above we expanded ISpecification purpose to become  
an instruction for a persistence logic behind IStateModifying to  
perform a transition from the current state to be matching the desired  
(See `ChangeDataToMatch` vs `MatchBeforeDataChanging` methods).  
You can learn more about the original read-only concept in any DDD book.

Finally, still keeping the DDD book open, what is an **entity**?  
To be more precise how the aggregate of your model corresponds  
to a modeled entity? It is the **identity** responsible for this.  
What is it, really? **No matter**. Even if it does matter and you need  
to perform some actions in your model with it - you should utilize  
domain services concept delegating the details away to the factories.

Precisely, digging more into the concept of entity, how do you  
"create" it? Factories? Not really. At least you don't need to build  
the whole record state by the factory, but rather you'd demand  
an identity for an "existed externally" entity to be registered 😲  
**Created or updated** - it **doesn't matter** for the model.  It will  
handle not yet "created" entity's record data as simple as a regular  
`LockedRecordData` instance with all properties unset 😲

Down the road we'll see how identities are represented and managed  
within OLTP records management, as well opening a brand new way of  
parameterizing your transactions. For now lets just keep in mind that  
identity structure is not a domain concern.

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

### Commit: 1|🏠: OLTP transactional composition

<table><tbody><tr><td>

1 file changed, 20 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.TransactionalComposition/</sub></sub><br>
<kbd> +++++++ 20  |⁠ [TransactionalCompositionTransactionCallback.cs                                                                 ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition/TransactionalCompositionTransactionCallback.cs)</kbd><br>

When it comes to building your aggregates for modelling the domain  
over a long period of time you often encounters them to be cluttered  
with logic and data serving still the same purpose the model exists  
but in different points of time and in different data volumes required.

The nowadays approach that comes in mind right away is "microservices"  
where you just need to split any concerns into separate apps. Sounds  
like a silver bullet? 😁 Hold on...

Before deciding to sell your soul to the devil, ask yourself:  

- whether the service I extract is going to bring the value solely, and;
- whether it's worth to lose the ACID guarantees for the sake of hype.

Long story short (again), even if you go down that road, your best  
bet is to prepare the code first before integrating it with the  
extracted service, i.e. to let the integration be served still... with  
a separated local module...

Now let's take it serious as a rule of thumb: "whenever we find a  
**subdomain** we first put it into a **separate assembly**". Do you  
see much difference in where the assembly is located, in the local  
code base or external? You might not yet, so no need to rush.

Give it a chance to stay and retain the ACID guarantees while  
utilizing the full-fledged modularity of your framework. For that we  
just need to introduce the way aggregates could communicate each other  
within the transaction 😲

Since we've already delegated the state access away from our model  
we have a way to identify which classes belong to the subset of those  
aggregates. I.e. the ones getting `IStateAccess` in the constructor.  
> Please note this once again in contrast to the ORM's approach where  
you pass dozens of ctor parameters to obtaining the required state 🙈

So our ctor is not so busy now, right? Why not to "ask it" to simply  
accept other aggregates as dependencies, huh? Do you feel how it  
smells like the sweet origins of OOP? 😁 Do you see how the language's  
natural semantics are going to be put at the core of making clusters   
of such aggregates "describing a single entity yet" 🤔 Hopefuly from  
now on we're on the same page 🤞

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

### Commit: 2|🏠: OLTP transaction is limited size

<table><tbody><tr><td>

20 files changed, 1500 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement/</sub></sub><br>
<kbd> +++++++ 167 |⁠ [IRecordsBatch.cs                                                                                               ](src/YourCompany.OLTP.RecordsManagement/IRecordsBatch.cs)</kbd><br>
<kbd>   +++++ 135 |⁠ [RecordsBatchTransactionCallback.cs                                                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionCallback.cs)</kbd><br>
<kbd>   +++++ 125 |⁠ [RecordsBatchTransactionConfiguration.ByIds.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.ByIds.cs)</kbd><br>
<kbd>   +++++ 125 |⁠ [RecordsBatchTransactionConfiguration.NoSorting.cs                                                              ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.NoSorting.cs)</kbd><br>
<kbd>    ++++ 98  |⁠ [RecordsBatchTransactionConfiguration.NoSorting.Paginated.cs                                                    ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.NoSorting.Paginated.cs)</kbd><br>
<kbd>    ++++ 90  |⁠ [RecordsBatchTransactionSpecification.ReadOnlyIncompatible.cs                                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnlyIncompatible.cs)</kbd><br>
<kbd>    ++++ 90  |⁠ [RecordsBatchTransactionExtensions.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionExtensions.cs)</kbd><br>
<kbd>    ++++ 85  |⁠ [RecordsBatchTransactionConfiguration.ById.cs                                                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.ById.cs)</kbd><br>
<kbd>     +++ 83  |⁠ [RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord.cs                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord.cs)</kbd><br>
<kbd>     +++ 81  |⁠ [RecordsBatchTransactionConfiguration.AfterId.cs                                                                ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.AfterId.cs)</kbd><br>
<kbd>     +++ 76  |⁠ [RecordsBatchTransactionConfiguration.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.cs)</kbd><br>
<kbd>     +++ 73  |⁠ [RecordsBatchTransactionSpecification.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.cs)</kbd><br>
<kbd>     +++ 64  |⁠ [RecordsBatchTransactionConfiguration.AfterId.Paginated.cs                                                      ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionConfiguration.AfterId.Paginated.cs)</kbd><br>
<kbd>      ++ 51  |⁠ [RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible.cs                                            ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible.cs)</kbd><br>
<kbd>      ++ 50  |⁠ [RecordsBatchTransactionSpecification.Sorting.cs                                                                ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.Sorting.cs)</kbd><br>
<kbd>      ++ 47  |⁠ [RecordsBatchTransactionSpecificationExtensions.cs                                                              ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecificationExtensions.cs)</kbd><br>
<kbd>       + 23  |⁠ [RecordsBatchTransaction.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.cs)</kbd><br>
<kbd>       + 20  |⁠ [RecordsBatchTransactionSpecification.ReadOnly.cs                                                               ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnly.cs)</kbd><br>
<kbd>       + 10  |⁠ [IRepository.cs                                                                                                 ](src/YourCompany.OLTP.RecordsManagement/IRepository.cs)</kbd><br>
<kbd>       + 7   |⁠ [IRecordsBatchSizeLimit.cs                                                                                      ](src/YourCompany.OLTP.RecordsManagement/IRecordsBatchSizeLimit.cs)</kbd><br>

Long story short big transactions work bad. We need a way  
to enforce the records number limit per any transaction run.

But the size is not the only "specification" to be considered.  
In contrast to the DDD specifications these are the options  
rather than the matching strategy. There are particular rules that  
must be followed to be able to resolve a particular combination  
of these options. Look for details in these rules implemented by  
`RecordsBatchTransactionSpecification` semi-closed hierarchy.

Considering the compatibility rules and their nested long-name nature  
its creation is streamlined into a fluent-API-like extensions set  
in `RecordsBatchTransactionExtensions`.

Among all it's worth to highlight use cases and keyset pagination.

For use cases as we stated before it is typical to modify data.  
That's why we design `UseCaseParameters` to switch any transaction  
into modifying one. It is expected to declare a **dedicated class per  
use case**. It may contain some previously queried data or represent a  
domain event. But there is also a special case allowing to turn any  
transaction into modifying without any data being changed and later we  
will see why. The special case is triggered by  
`TriggeredBeforeDataChanging` event args, and there are also a few  
`ReadAndMatchExclusive` extension methods.

For keyset pagination. It tightly relates to records ordering. To be  
effective that ordering must be performed on the storage side.  
That means we need to instruct the appropriate persistence adapter  
to pick records after a particular unique key in a particular order.  
And It turns out that previously introduced Identity is a perfect fit 🤔

The model asks a particular factory to provide a key `Identity`  
for values it specifies while encapsulating a desired ordering by the  
factory method name or extra parameters. This way all the persistence  
concerns remain isolated from the model. If your use case or a  
"service 🙈" logic requires to iterate more records through than the  
records batch size limit allows then it may use the  
`TryGetLastPresentRecordIdentity` extension to continue in another  
transaction. Such key identity is sometimes called a "cursor".

</td></tr></tbody></table>

<!-- ### Commit: 2|🏠: OLTP transaction is limited size END -->

### Commit: 2|🧱: StructHelper

<table><tbody><tr><td>

1 file changed, 18 insertions(+)<br>
<sub><sub>src/YourCompany.CompilerServices/</sub></sub><br>
<kbd> +++++++ 18  |⁠ [StructHelper.cs                                                                                                ](src/YourCompany.CompilerServices/StructHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: StructHelper END -->

### Commit: 2|🧱: AwaitTasksList

<table><tbody><tr><td>

1 file changed, 49 insertions(+)<br>
<sub><sub>src/YourCompany.Threading/</sub></sub><br>
<kbd> +++++++ 49  |⁠ [AwaitTasksList.cs                                                                                              ](src/YourCompany.Threading/AwaitTasksList.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: AwaitTasksList END -->

### Commit: 2|🧱: GetMemberHelper

<table><tbody><tr><td>

1 file changed, 57 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection/</sub></sub><br>
<kbd> +++++++ 57  |⁠ [GetMemberHelper.cs                                                                                             ](src/YourCompany.Reflection/GetMemberHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: GetMemberHelper END -->

### Commit: 2|🧱: DIConstructorHelper

<table><tbody><tr><td>

1 file changed, 31 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection.DI/</sub></sub><br>
<kbd> +++++++ 31  |⁠ [DIConstructorHelper.cs                                                                                         ](src/YourCompany.Reflection.DI/DIConstructorHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: DIConstructorHelper END -->

### Commit: 2|🧱: ValueTupleHelper

<table><tbody><tr><td>

2 files changed, 143 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection/</sub></sub><br>
<kbd> +++++++ 113 |⁠ [ValueTupleHelper.Expressions.cs                                                                                ](src/YourCompany.Reflection/ValueTupleHelper.Expressions.cs)</kbd><br>
<kbd>      ++ 30  |⁠ [ValueTupleHelper.cs                                                                                            ](src/YourCompany.Reflection/ValueTupleHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: ValueTupleHelper END -->

### Commit: 2|🧱: TypeAbstractionsHelper

<table><tbody><tr><td>

1 file changed, 26 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection/</sub></sub><br>
<kbd> +++++++ 26  |⁠ [TypeAbstractionsHelper.cs                                                                                      ](src/YourCompany.Reflection/TypeAbstractionsHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|🧱: TypeAbstractionsHelper END -->

### Commit: 2|🧱: TypesCompositionMap

<table><tbody><tr><td>

16 files changed, 1050 insertions(+)<br>
<sub><sub>src/YourCompany.Reflection/Composition/</sub></sub><br>
<kbd> +++++++ 141 |⁠ [ComposableTypeInfo.ConstructionLimitations.cs                                                                  ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.ConstructionLimitations.cs)</kbd><br>
<kbd>  ++++++ 139 |⁠ [ComposableTypeInfo.Dependencies.cs                                                                             ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.Dependencies.cs)</kbd><br>
<kbd>  ++++++ 133 |⁠ [ComposableTypeInfo.cs                                                                                          ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.cs)</kbd><br>
<kbd>   +++++ 113 |⁠ [CompositionException.Composite.cs                                                                              ](src/YourCompany.Reflection/Composition/CompositionException.Composite.cs)</kbd><br>
<kbd>   +++++ 104 |⁠ [ComposableTypeInfo.Dependencies.Mixins.Determined.cs                                                           ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.Dependencies.Mixins.Determined.cs)</kbd><br>
<kbd>    ++++ 82  |⁠ [ComposableTypeInfo.Dependencies.Mixins.CrossCutting.cs                                                         ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.Dependencies.Mixins.CrossCutting.cs)</kbd><br>
<kbd>    ++++ 72  |⁠ [TypesCompositionMap.cs                                                                                         ](src/YourCompany.Reflection/Composition/TypesCompositionMap.cs)</kbd><br>
<kbd>      ++ 41  |⁠ [ComposableTypeInfo.Dependencies.ImplementedAbstractions.cs                                                     ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.Dependencies.ImplementedAbstractions.cs)</kbd><br>
<kbd>      ++ 39  |⁠ [CompositionException.DependenciesLoopDetected.cs                                                               ](src/YourCompany.Reflection/Composition/CompositionException.DependenciesLoopDetected.cs)</kbd><br>
<kbd>      ++ 36  |⁠ [ComposableTypeInfo.Dependencies.Mixins.cs                                                                      ](src/YourCompany.Reflection/Composition/ComposableTypeInfo.Dependencies.Mixins.cs)</kbd><br>
<kbd>      ++ 31  |⁠ [CompositionException.AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable.cs         ](src/YourCompany.Reflection/Composition/CompositionException.AbstractDependencyWithMultipleImplementationsPerApplicableRootMustBeEnumerable.cs)</kbd><br>
<kbd>      ++ 30  |⁠ [CompositionException.CrossCuttingMixinMustOnlyDependOnOtherCrossCutting.cs                                     ](src/YourCompany.Reflection/Composition/CompositionException.CrossCuttingMixinMustOnlyDependOnOtherCrossCutting.cs)</kbd><br>
<kbd>      ++ 29  |⁠ [CompositionException.ProvidedComposableBaseTypesMustBeAbstract.cs                                              ](src/YourCompany.Reflection/Composition/CompositionException.ProvidedComposableBaseTypesMustBeAbstract.cs)</kbd><br>
<kbd>      ++ 24  |⁠ [CompositionException.cs                                                                                        ](src/YourCompany.Reflection/Composition/CompositionException.cs)</kbd><br>
<kbd>      ++ 24  |⁠ [ComposableTypesProvider.cs                                                                                     ](src/YourCompany.Reflection/Composition/ComposableTypesProvider.cs)</kbd><br>
<kbd>       + 12  |⁠ [CompositionException.MixinMustHaveApplicableRoots.cs                                                           ](src/YourCompany.Reflection/Composition/CompositionException.MixinMustHaveApplicableRoots.cs)</kbd><br>

While not as widespreadly known paradigm as others,  
**Subject-Oriented-Programming** still managed to leave a positive  
mark on the industry history, and its echoes are found not only in its  
overshadowed AOP paradigm but also in several programming languages  
in the limited form of "mixins" or "traits".

Long story short (though higly recommend introducing yourself to this)  
and as absurds as it sounds... True modularity sometimes is impossible  
for one and very simple reason - you can't simply cut of your class  
into smaller pieces without having to glue them back together elsewhere.

When you come into this problem you have few options, patterns like  
"bridge", or loosing strong typing benefits by wrapping such pieces  
into kind of "memento" (dictionary, or alike, e.g. underlying form of  
mixing in JS) and some other language and runtime abilities like  
stateful extensions in C# utilizing "weak references", etc...

As practice shows SOP is way more complex than OOP in a way presented,  
i.e. as a paradigm, and the aforementioned ways of "mixing" too.  
But what's interesting, and what we actually often miss is how  
natural it sounds when you try building some descriptive model with it  
rather trying to far-fetch it into an arbitrary procedural processing.

Later we'll see this paradigm evolving into the practice while being  
adopted into your framework for more focused goals. For now let's just  
keep in mind that it's all about composition. And now we're organizing  
the map serving as a basement for your framework to collect and  
determine relationships between the pieces.

Implementation hint: at `ComposableTypesProvider` you can see that  
objects' construction is abstracted away meaning that the exact way  
of instantiation will be taken over by another module in the framework.

Also worth mentioning the cross cutting mixins that the provider is  
responsible to denote for. These are the closest to AOP aspects,  
but are not requiring to declare the "extensions point" explicitly to  
be applicable for composition.

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

### Commit: 2|💾: EFCore pessimistic locking

<table><tbody><tr><td>

7 files changed, 328 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/PessimisticLocking/</sub></sub><br>
<kbd> +++++++ 117 |⁠ [ProviderSpecificPessimisticLockingUpdateInterceptor.cs                                                         ](src/YourCompany.Configuration.EFCore/PessimisticLocking/ProviderSpecificPessimisticLockingUpdateInterceptor.cs)</kbd><br>
<kbd>    ++++ 60  |⁠ [PessimisticLockingUpdateInterceptionContext.cs                                                                 ](src/YourCompany.Configuration.EFCore/PessimisticLocking/PessimisticLockingUpdateInterceptionContext.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.PostgreSQL/</sub></sub><br>
<kbd>     +++ 58  |⁠ [NpgsqlPessimisticLockingUpdateInterceptor.cs                                                                   ](src/YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlPessimisticLockingUpdateInterceptor.cs)</kbd><br>
<kbd>       + 1   |⁠ [NpgsqlDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlDbContextConfigurator.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore.Sqlite/</sub></sub><br>
<kbd>     +++ 46  |⁠ [SqlitePessimisticLockingUpdateInterceptor.cs                                                                   ](src/YourCompany.Configuration.EFCore.Sqlite/SqlitePessimisticLockingUpdateInterceptor.cs)</kbd><br>
<kbd>       + 1   |⁠ [SqliteDbContextConfigurator.cs                                                                                 ](src/YourCompany.Configuration.EFCore.Sqlite/SqliteDbContextConfigurator.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd>     +++ 45  |⁠ [YourCompanyDbContext.PessimisticLocking.cs                                                                     ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.PessimisticLocking.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 2|💾: EFCore pessimistic locking END -->

### Commit: 2|💾: EFChangeTrackerTrackGraphStrategy

<table><tbody><tr><td>

1 file changed, 106 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/ChangeTracking/</sub></sub><br>
<kbd> +++++++ 106 |⁠ [EFChangeTrackerTrackGraphStrategy.cs                                                                           ](src/YourCompany.Configuration.EFCore/ChangeTracking/EFChangeTrackerTrackGraphStrategy.cs)</kbd><br>

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

### Commit: 3|🏠: OLTP RecordTypesMap

<table><tbody><tr><td>

4 files changed, 263 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.Reflection/</sub></sub><br>
<kbd> +++++++ 97  |⁠ [RecordConstructionHelper.cs                                                                                    ](src/YourCompany.OLTP.StateOwnership.Reflection/RecordConstructionHelper.cs)</kbd><br>
<kbd>   +++++ 74  |⁠ [RecordTypeInfo.cs                                                                                              ](src/YourCompany.OLTP.StateOwnership.Reflection/RecordTypeInfo.cs)</kbd><br>
<kbd>    ++++ 56  |⁠ [RecordConstructionException.cs                                                                                 ](src/YourCompany.OLTP.StateOwnership.Reflection/RecordConstructionException.cs)</kbd><br>
<kbd>     +++ 36  |⁠ [RecordTypesMap.cs                                                                                              ](src/YourCompany.OLTP.StateOwnership.Reflection/RecordTypesMap.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP RecordTypesMap END -->

### Commit: 3|🏠: generic OLTP use cases

<table><tbody><tr><td>

2 files changed, 212 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.UseCases/</sub></sub><br>
<kbd> +++++++ 167 |⁠ [RecordsBatchTransactionUseCases.RunningExtensions.cs                                                           ](src/YourCompany.OLTP.RecordsManagement.UseCases/RecordsBatchTransactionUseCases.RunningExtensions.cs)</kbd><br>
<kbd>      ++ 45  |⁠ [RecordsBatchTransactionUseCases.cs                                                                             ](src/YourCompany.OLTP.RecordsManagement.UseCases/RecordsBatchTransactionUseCases.cs)</kbd><br>

Easily reuse and combine authorizers, record property setters or  
organize each transaction post processing and extend aggregate  
boundaries (though still limited with module boundaries, we'll  
improve this later).

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: generic OLTP use cases END -->

### Commit: 3|🏠: OLTP UseCaseTypesMap

<table><tbody><tr><td>

2 files changed, 176 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.UseCases.Reflection/</sub></sub><br>
<kbd> +++++++ 89  |⁠ [UseCaseTypeInfo.cs                                                                                             ](src/YourCompany.OLTP.RecordsManagement.UseCases.Reflection/UseCaseTypeInfo.cs)</kbd><br>
<kbd>  ++++++ 87  |⁠ [UseCaseTypesMap.cs                                                                                             ](src/YourCompany.OLTP.RecordsManagement.UseCases.Reflection/UseCaseTypesMap.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP UseCaseTypesMap END -->

### Commit: 3|🏠: OLTP RecordTypesCompositionMap

<table><tbody><tr><td>

5 files changed, 260 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/</sub></sub><br>
<kbd> +++++++ 81  |⁠ [ComposableRecordTypesProvider.cs                                                                               ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/ComposableRecordTypesProvider.cs)</kbd><br>
<kbd>   +++++ 62  |⁠ [ComposableRecordTypeInfo.cs                                                                                    ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/ComposableRecordTypeInfo.cs)</kbd><br>
<kbd>    ++++ 49  |⁠ [RecordTypesCompositionMap.cs                                                                                   ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/RecordTypesCompositionMap.cs)</kbd><br>
<kbd>     +++ 35  |⁠ [RecordCompositionException.cs                                                                                  ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/RecordCompositionException.cs)</kbd><br>
<kbd>     +++ 33  |⁠ [RecordCompositionToConstructionWrapperException.cs                                                             ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection/RecordCompositionToConstructionWrapperException.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 3|🏠: OLTP RecordTypesCompositionMap END -->

### Commit: 3|🧱: OLTP transaction structure

<table><tbody><tr><td>

17 files changed, 1919 insertions(+), 1 deletion(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement/</sub></sub><br>
<kbd> +++++++ 233 |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.cs                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.cs)</kbd><br>
<kbd>  ++++++ 212 |⁠ [RecordsBatchTransaction.Specified.SetOnceProperties.cs                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.SetOnceProperties.cs)</kbd><br>
<kbd>   +++++ 174 |⁠ [RecordsBatchTransaction.WithIdentities.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.cs)</kbd><br>
<kbd>   +++++ 172 |⁠ [RecordsBatchTransaction.Specified.Handling.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.Handling.cs)</kbd><br>
<kbd>   +++++ 160 |⁠ [RecordsBatchTransaction.SpecifiedRun.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.cs)</kbd><br>
<kbd>    ++++ 154 |⁠ [RecordsBatchTransaction.WithIdentities.ReplacingSpecified.cs                                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.ReplacingSpecified.cs)</kbd><br>
<kbd>    ++++ 137 |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.RecordState.cs                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.RecordState.cs)</kbd><br>
<kbd>    ++++ 132 |⁠ [RecordState.cs                                                                                                 ](src/YourCompany.OLTP.RecordsManagement/RecordState.cs)</kbd><br>
<kbd>     +++ 107 |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.TriggerTransactionCallback.cs                              ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.TriggerTransactionCallback.cs)</kbd><br>
<kbd>     +++ 106 |⁠ [RecordsBatchTransaction.SpecifiedRun.State.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.State.cs)</kbd><br>
<kbd>     +++ 95  |⁠ [RecordsBatchTransaction.WithIdentities.Unique.cs                                                               ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.Unique.cs)</kbd><br>
<kbd>      ++ 59  |⁠ [RecordsBatchTransaction.Specified.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.cs)</kbd><br>
<kbd>      ++ 58  |⁠ [RecordsBatchTransaction.SpecifiedRun.TypicalSequence.cs                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.TypicalSequence.cs)</kbd><br>
<kbd>      ++ 58  |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.BuiltRecord.cs                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.BuiltRecord.cs)</kbd><br>
<kbd>      ++ 39  |⁠ [RecordsBatchTransaction.WithRecords.cs                                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.cs)</kbd><br>
<kbd>       + 22  |⁠ [RecordsBatchTransaction.SpecifiedRun.RunOnceStep.cs                                                            ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.RunOnceStep.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.cs)</kbd><br>

Be ready to implement following sequence per each record type:

1. `MaxRecordsInBatch`
2. `HandleSpecifiedIdentities`
3. `HandleSortingAfterId`
4. `Authorize`
5. `HandleReadOnlyRecordsIncludingBeforeRead`
6. `HandleEachRecordSpecificationBeforeReadWithoutIds`
7. `HandleEachRecordModifyingSpecificationBeforeRead`
8. `ReadWithHandledEachRecordSpecifications`
9. `GetReadRecordIdentity`
10. `ValidateAssignedIdentityReplacementAfterReadByIds`
11. `BuildRecord`
12. `CreateRecordDataForSettingChangedProperties`
13. `PrepareRecordsBatchChanges`
14. `HandleSpecifiedRecordModifyingSpecificationBeforePersist`
15. `PersistChanges`
16. `GetLockedRecordDataWithoutChanges`
17. `FinishRecordsDataAccess`
18. `GetRecordDataAfterAccess`
19. `HandleResultingRecordsBatch`

Including a bunch of virtual methods, these give you ability to pause  
between steps, make extra processing, etc. In short this entry point  
is good for reimagining the framework in your own way. Later we're  
going to significantly reduce the number of required methods to be  
implemented by introducing DI and by segregating use case and  
persistence related concerns.

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

### Commit: 4|🏠: enumerate OLTP use cases

<table><tbody><tr><td>

2 files changed, 117 insertions(+)<br>
<sub><sub><del>src/YourCompany.OLTP.RecordsManagement.UseCases/</del></sub></sub><br>
<kbd> +++++++ 64  |⁠ RecordsBatchTransaction.UseCasesParallelRun.cs                                                                 </kbd><br>
<kbd>   +++++ 53  |⁠ RecordsBatchTransaction.cs                                                                                     </kbd><br>

We've segregated use case related concerns into several interfaces.

The set of methods to override is slightly reduced for  
`RecordsBatchTransaction` implementors:

1. `MaxRecordsInBatch`
2. `HandleSpecifiedIdentities`
3. `HandleSortingAfterId`
4. `HandleReadOnlyRecordsIncludingBeforeRead`
5. `HandleEachRecordSpecificationBeforeReadWithoutIds`
6. `HandleEachRecordModifyingSpecificationBeforeRead`
7. `ReadWithHandledEachRecordSpecifications`
8. `GetReadRecordIdentity`
9. `ValidateAssignedIdentityReplacementAfterReadByIds`
10. `BuildRecord`
11. `CreateRecordDataForSettingChangedProperties`
12. `HandleSpecifiedRecordModifyingSpecificationBeforePersist`
13. `PersistChanges`
14. `GetLockedRecordDataWithoutChanges`
15. `FinishRecordsDataAccess`
16. `GetRecordDataAfterAccess`

If your concern is reducing the number of methods and/or simplifying  
persistence logic consider the alternative entry point branch  
`oltp-segregate-persistence-repository-is-for-use-cases` where the  
persistent logic is bridged into a state-sequenced set of  
interfaces, making it much more streamlined to implement persistence.

> To make both approaches work together we'll next introduce a DI  
facade which will allow to avoid inheriting `RecordsBatchTransaction`  
completely.

</td></tr></tbody></table>

<!-- ### Commit: 4|🏠: enumerate OLTP use cases END -->

### Commit: 4|🏠: OLTP use cases by record type

<table><tbody><tr><td>

8 files changed, 770 insertions(+)<br>
<sub><sub><del>src/YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition/</del></sub></sub><br>
<kbd> +++++++ 187 |⁠ ComposableRecordsBatchTransaction.IteratingInParallel.cs                                                       </kbd><br>
<kbd>  ++++++ 166 |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.cs                                                     </kbd><br>
<kbd>   +++++ 144 |⁠ ComposableRecordsBatchTransaction.ComposingRecords.cs                                                          </kbd><br>
<kbd>    ++++ 111 |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs                                   </kbd><br>
<kbd>      ++ 54  |⁠ AlwaysTrueSpecifications.cs                                                                                    </kbd><br>
<kbd>      ++ 43  |⁠ ComposableRecordsBatchTransaction.IteratedInParallel.cs                                                        </kbd><br>
<kbd>      ++ 36  |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs                                </kbd><br>
<kbd>       + 29  |⁠ ComposableRecordsBatchTransaction.cs                                                                           </kbd><br>

Here we utilize the ctor semantics and the previously introduced  
SOP-idea based composition to introduce the way aggregates can trigger  
each other's participation in a transaction. The way is to simply  
reuse the transaction callback capabilities, so for the aggregate  
being triggered to be no difference from handling regular use cases.  
The latter are identified by one of particlar `EventArgs` inheritors.

> The difference though and a recommendation is to separate regular  
use case args from the triggering callback args. For the sake of  
encapsulation it is best to keep regular ones "internal" while those  
for triggering "public".

Later we'll see how easy it is to add this new capability to the  
generic repository and how much the value brought increases wih DI.  
At current state you'll likely find it hard to implement those  
semantics manually by inheriting record batch transaction per each  
record type (here are both Entities and Mixins are meant). But  
hopefully the suggested hierarchy will help, e.g. for adapting another  
DI lib.

</td></tr></tbody></table>

<!-- ### Commit: 4|🏠: OLTP use cases by record type END -->

### Commit: 4|🧱: OLTP identities are unique keys

<table><tbody><tr><td>

11 files changed, 1642 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence/</sub></sub><br>
<kbd> +++++++ 257 |⁠ [RecordsDataAccess.Proxy.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.Proxy.cs)</kbd><br>
<kbd>  ++++++ 238 |⁠ [RecordsDataAccess.State.FinishingChain.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.FinishingChain.cs)</kbd><br>
<kbd>  ++++++ 227 |⁠ [RecordsBatchTransactionDataBridge.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsBatchTransactionDataBridge.cs)</kbd><br>
<kbd>  ++++++ 225 |⁠ [RecordsDataAccess.State.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.cs)</kbd><br>
<kbd>    ++++ 171 |⁠ [UniqueKey.Primary.cs                                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence/UniqueKey.Primary.cs)</kbd><br>
<kbd>    ++++ 157 |⁠ [RecordsDataAccess.State.ReadIdentity.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.ReadIdentity.cs)</kbd><br>
<kbd>    ++++ 130 |⁠ [RecordsDataAccess.cs                                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.cs)</kbd><br>
<kbd>      ++ 85  |⁠ [RecordsDataAccess.State.AfterSorting.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.AfterSorting.cs)</kbd><br>
<kbd>      ++ 61  |⁠ [UniqueKey.Alternate.cs                                                                                         ](src/YourCompany.OLTP.RecordsManagement.Persistence/UniqueKey.Alternate.cs)</kbd><br>
<kbd>      ++ 50  |⁠ [UniqueKey.cs                                                                                                   ](src/YourCompany.OLTP.RecordsManagement.Persistence/UniqueKey.cs)</kbd><br>
<kbd>       + 41  |⁠ [UniqueKeyExtensions.cs                                                                                         ](src/YourCompany.OLTP.RecordsManagement.Persistence/UniqueKeyExtensions.cs)</kbd><br>

These might be primary, natural, or... prefixed. Remember afterId?  
Keyset pagination? When we sort by some data columns we could  
not necessarily use unique tuples, but rather just add a primary key  
still allowing to utilize keyset pagination. Either way the query  
result is expected to expose the so called "cursor" having the same  
structure as the key parameter (afterId).

The mistake № 2 is to expose generated sequential ids, store,  
serialize it outside the original storage, its btw a security concern 👻

The mistake № 3 is to violate aggregate boundaries by putting extra  
foreign keys between aggregates still using those sequential ids.

UniqueKey semi-closed hierarchy takes into account all of these  
and implements the major rules of identity lifecycle, such as  
separating public and private PK as well as using predetermined  
optimal PK types, i.e. `long` to be tolerant to continuous insertion  
errors and `Guid` to be easily serializable and unified in a future  
API surface.

As for `RecordsBatchTransaction`, or more precisely  
`RecordsBatchTransactionDataBridge` bridging the state-sequenced set  
of data access interfaces, you still need to implement several  
required methods:

1. `Authorize`
2. `BuildRecord`
3. `PrepareRecordsBatchChanges`
4. `HandleResultingRecordsBatch`

These might be still not desired. You may find more useful the  
oltp-segregate-use-cases-repository-is-for-persistence branch.

> To make both approaches work together we'll next introduce a DI  
facade which will allow to avoid inheriting `RecordsBatchTransaction`  
completely.

</td></tr></tbody></table>

<!-- ### Commit: 4|🧱: OLTP identities are unique keys END -->

### Commit: 4|🧱: OLTP LINQ may be useful

<table><tbody><tr><td>

2 files changed, 59 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq/</sub></sub><br>
<kbd> +++++++ 49  |⁠ [RecordDataQueryBuilder.cs                                                                                      ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq/RecordDataQueryBuilder.cs)</kbd><br>
<kbd>      ++ 10  |⁠ [IRecordDataQueryBuildersProvider.cs                                                                            ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq/IRecordDataQueryBuildersProvider.cs)</kbd><br>

It's a small extension to the streamlined data access interfaces  
in the case one implements the transaction using linq-based  
libs like EF or linq2db. So that the query building and modifying  
specifications applying logic can be further decomposed.

</td></tr></tbody></table>

<!-- ### Commit: 4|🧱: OLTP LINQ may be useful END -->

### Commit: 4|💾: EFCore SortingKey reading

<table><tbody><tr><td>

10 files changed, 790 insertions(+), 1 deletion(-)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/</sub></sub><br>
<kbd> +++++++ 176 |⁠ [SortingKeyQueriesCache.ForQuery.ForTopology.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.ForTopology.cs)</kbd><br>
<kbd>  ++++++ 171 |⁠ [SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.cs                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.cs)</kbd><br>
<kbd>     +++ 86  |⁠ [SortingKeyQueriesCache.ForQuery.cs                                                                             ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.cs)</kbd><br>
<kbd>      ++ 58  |⁠ [SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.SingleEFProperty.cs                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.SingleEFProperty.cs)</kbd><br>
<kbd>      ++ 56  |⁠ [SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.MultiEFProperties.ForEntity.cs                     ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.MultiEFProperties.ForEntity.cs)</kbd><br>
<kbd>      ++ 51  |⁠ [SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.MultiEFProperties.cs                               ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ExpressionsCaching/SortingKeyQueriesCache.ForQuery.ForTopology.TopologyVisitor.MultiEFProperties.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd>   +++++ 132 |⁠ [YourCompanyDbContext.SingleEntityQueries.cs                                                                    ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.SingleEntityQueries.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>      ++ 51  |⁠ [SortingKeyQueries.SingleEntityExtensions.cs                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyQueries.SingleEntityExtensions.cs)</kbd><br>
<kbd>       + 8   |⁠ [SortingKeyExtraValuePair.cs                                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyExtraValuePair.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKeyQueries.cs                                                                                           ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyQueries.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFCore SortingKey reading END -->

### Commit: 4|💾: EFEntityEntryPropertiesCache

<table><tbody><tr><td>

1 file changed, 63 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/ChangeTracking/</sub></sub><br>
<kbd> +++++++ 63  |⁠ [EFEntityEntryPropertiesCache.cs                                                                                ](src/YourCompany.Configuration.EFCore/ChangeTracking/EFEntityEntryPropertiesCache.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFEntityEntryPropertiesCache END -->

### Commit: 4|💾: EFCore SortingKey ⇆ EntityEntry

<table><tbody><tr><td>

11 files changed, 764 insertions(+), 1 deletion(-)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/</sub></sub><br>
<kbd> +++++++ 127 |⁠ [EntityEntryPropertiesCopyingVisitor.cs                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.cs)</kbd><br>
<kbd>  ++++++ 116 |⁠ [EntityEntryPropertiesCopyingVisitor.NonGeneric.cs                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.NonGeneric.cs)</kbd><br>
<kbd>  ++++++ 110 |⁠ [EntityEntrySortingKeyCreatingVisitor.cs                                                                        ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.cs)</kbd><br>
<kbd>   +++++ 105 |⁠ [EntityEntryPropertiesCopyingVisitor.Generic.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.Generic.cs)</kbd><br>
<kbd>    ++++ 64  |⁠ [EntityEntrySortingKeyCreatingVisitor.NonGeneric.cs                                                             ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.NonGeneric.cs)</kbd><br>
<kbd>     +++ 61  |⁠ [EntityEntryPropertiesSettingVisitor.NonGeneric.cs                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.NonGeneric.cs)</kbd><br>
<kbd>     +++ 60  |⁠ [EntityEntryPropertiesSettingVisitor.Generic.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.Generic.cs)</kbd><br>
<kbd>     +++ 55  |⁠ [EntityEntrySortingKeyCreatingVisitor.Generic.cs                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.Generic.cs)</kbd><br>
<kbd>     +++ 48  |⁠ [EntityEntryPropertiesSettingVisitor.cs                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>       + 17  |⁠ [SortingKeyTopologyExtensions.ChangeTracking.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopologyExtensions.ChangeTracking.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKeyTopologyExtensions.cs                                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopologyExtensions.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: EFCore SortingKey ⇆ EntityEntry END -->

### Commit: 4|💾: OLTP EFCore MetadataHelper

<table><tbody><tr><td>

1 file changed, 45 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/</sub></sub><br>
<kbd> +++++++ 45  |⁠ [MetadataHelper.cs                                                                                              ](src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/MetadataHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: OLTP EFCore MetadataHelper END -->

### Commit: 4|💾: OLTP EFCore ID generation

<table><tbody><tr><td>

4 files changed, 112 insertions(+), 1 deletion(-)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/</sub></sub><br>
<kbd> +++++++ 79  |⁠ [MetadataHelper.DelegatedPrimaryKeyGeneration.cs                                                                ](src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/MetadataHelper.DelegatedPrimaryKeyGeneration.cs)</kbd><br>
<kbd>      ++ 21  |⁠ [RecordDataQueries.cs                                                                                           ](src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/RecordDataQueries.cs)</kbd><br>
<kbd>       + 11  |⁠ [DelegatedPrimaryKeyGenerationTable.cs                                                                          ](src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/DelegatedPrimaryKeyGenerationTable.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [MetadataHelper.cs                                                                                              ](src/YourCompany.OLTP.StateOwnership.Reflection.EFCore/MetadataHelper.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 4|💾: OLTP EFCore ID generation END -->

### Commit: 4|💾: EFEntityTypeSortingKeyTopology

<table><tbody><tr><td>

7 files changed, 347 insertions(+)<br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/</sub></sub><br>
<kbd> +++++++ 90  |⁠ [EFEntityTypeSortingKeyTopology.Visiting.cs                                                                     ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/EFEntityTypeSortingKeyTopology.Visiting.cs)</kbd><br>
<kbd>  ++++++ 83  |⁠ [EFEntityTypeSortingKeyTopology.PrefixFirstPropertiesVisitCache.cs                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/EFEntityTypeSortingKeyTopology.PrefixFirstPropertiesVisitCache.cs)</kbd><br>
<kbd>  ++++++ 77  |⁠ [EFEntityTypeSortingKeyTopology.cs                                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/EFEntityTypeSortingKeyTopology.cs)</kbd><br>
<kbd>       + 11  |⁠ [CollationAwareSortingSingleModelProvider.cs                                                                    ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/CollationAwareSortingSingleModelProvider.cs)</kbd><br>
<kbd>       + 4   |⁠ [ModelExtensions.cs                                                                                             ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/ModelExtensions.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>  ++++++ 80  |⁠ [SortingKeyTopology.PrefixFirstPropertiesVisitCache.cs                                                          ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyTopology.PrefixFirstPropertiesVisitCache.cs)</kbd><br>
<kbd>       + 2   |⁠ [ICollationAwareModelProvider.cs                                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ICollationAwareModelProvider.cs)</kbd><br>

An edge case of the topology is all the entity properties.  
Using previously introduced visiting abilities it will allow to  
optimally implement some missing operations in EF.

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

### Commit: 5|🧱: OLTP switch to chassis

<table><tbody><tr><td>

35 files changed, 298 insertions(+), 312 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement/</sub></sub><br>
<kbd>     +‑‑ 30  |⁠ [RecordsBatchTransaction.SpecifiedRun.State.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.State.cs)</kbd><br>
<kbd>     +‑‑ 28  |⁠ [RecordsBatchTransactionCallback.cs                                                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionCallback.cs)</kbd><br>
<kbd>     +‑‑ 22  |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.BuiltRecord.cs                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.BuiltRecord.cs)</kbd><br>
<kbd>      +‑ 16  |⁠ [RecordState.cs                                                                                                 ](src/YourCompany.OLTP.RecordsManagement/RecordState.cs)</kbd><br>
<kbd>      +‑ 14  |⁠ [RecordsBatchTransactionSpecification.ReadOnlyIncompatible.cs                                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnlyIncompatible.cs)</kbd><br>
<kbd>      +‑ 14  |⁠ [RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible.cs                                            ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible.cs)</kbd><br>
<kbd>      +‑ 12  |⁠ [RecordsBatchTransaction.Specified.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord.cs                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [RecordsBatchTransactionSpecification.Sorting.cs                                                                ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.Sorting.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [RecordsBatchTransaction.SpecifiedRun.RunOnceStep.cs                                                            ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.RunOnceStep.cs)</kbd><br>
<kbd>      +‑ 8   |⁠ [RecordsBatchTransactionSpecification.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.cs                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [RecordsBatchTransactionSpecification.ReadOnly.cs                                                               ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecification.ReadOnly.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.TriggerTransactionCallback.cs                              ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.TriggerTransactionCallback.cs)</kbd><br>
<kbd>      +‑ 5   |⁠ [RecordsBatchTransaction.WithIdentities.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [RecordsBatchTransaction.WithRecords.WithRecordsData.RecordState.cs                                             ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.WithRecordsData.RecordState.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [RecordsBatchTransaction.SpecifiedRun.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [RecordsBatchTransactionSpecificationExtensions.cs                                                              ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransactionSpecificationExtensions.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.Specified.Handling.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.Handling.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.SpecifiedRun.TypicalSequence.cs                                                        ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.SpecifiedRun.TypicalSequence.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.WithRecords.cs                                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithRecords.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.Specified.SetOnceProperties.cs                                                         ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.Specified.SetOnceProperties.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.WithIdentities.Unique.cs                                                               ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.Unique.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.WithIdentities.ReplacingSpecified.cs                                                   ](src/YourCompany.OLTP.RecordsManagement/RecordsBatchTransaction.WithIdentities.ReplacingSpecified.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence/</sub></sub><br>
<kbd>   ++‑‑‑ 50  |⁠ [RecordsDataAccess.Proxy.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.Proxy.cs)</kbd><br>
<kbd>     +‑‑ 32  |⁠ [RecordsDataAccess.State.FinishingChain.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.FinishingChain.cs)</kbd><br>
<kbd>      +‑ 20  |⁠ [RecordsDataAccess.State.ReadIdentity.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.ReadIdentity.cs)</kbd><br>
<kbd>      +‑ 14  |⁠ [RecordsDataAccess.State.AfterSorting.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.AfterSorting.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [RecordsDataAccess.State.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsDataAccess.State.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransactionDataBridge.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence/RecordsBatchTransactionDataBridge.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.DI/</sub></sub><br>
<kbd> +++++++ 65  |⁠ [RecordsBatchTransaction.UseCasesParallelRun.cs                                                                 ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.UseCasesParallelRun.cs)</kbd><br>
<kbd>  ++++++ 57  |⁠ [RecordsBatchTransaction.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.cs)</kbd><br>
<sub><sub><del>src/YourCompany.OLTP.RecordsManagement.UseCases/</del></sub></sub><br>
<kbd>  ‑‑‑‑‑‑ 64  |⁠ RecordsBatchTransaction.UseCasesParallelRun.cs                                                                 </kbd><br>
<kbd>   ‑‑‑‑‑ 53  |⁠ RecordsBatchTransaction.cs                                                                                     </kbd><br>
<sub><sub>src/YourCompany.OLTP.StateOwnership/</sub></sub><br>
<kbd>     ++‑ 22  |⁠ [TransactionCallback.cs                                                                                         ](src/YourCompany.OLTP.StateOwnership/TransactionCallback.cs)</kbd><br>

Here we internalize a lot of public classes and methods previously  
open for overriding per project per entity. From now on we're  
committed to classify any outstanding needs first while making  
adjustments in your owned enterprise chassis repo (i.e. providing  
nuget packages, ideally).

This allow us to switch into much more powerful approaches while  
keeping the project's code as thin as possible, getting the most out  
of the rule **The less code doing the job, the more value it brings**.  
To put it simple, we want our projects to utilize the most out of  
OOP/DDD with as less as possible infrastructure, glue, facading code.  
In new features we're going to rely more on configurations for  
potential overrides rather than implement them each time.

Note that `RecordsBatchTransaction` is made abstract temporarily. Next  
we are going to introduce some reflection to completely avoid  
its inheritance as well as utilize the semantics we already introduced  
in the very beginning for our models accessing their state in a  
delegated manner.

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP switch to chassis END -->

### Commit: 5|🧱: OLTP DI ScopedRecordsProvider

<table><tbody><tr><td>

4 files changed, 120 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.Reflection.DI/</sub></sub><br>
<kbd> +++++++ 80  |⁠ [ScopedRecordsProvider.cs                                                                                       ](src/YourCompany.OLTP.StateOwnership.Reflection.DI/ScopedRecordsProvider.cs)</kbd><br>
<kbd>      ++ 19  |⁠ [SingletonStateAccess.cs                                                                                        ](src/YourCompany.OLTP.StateOwnership.Reflection.DI/SingletonStateAccess.cs)</kbd><br>
<kbd>       + 12  |⁠ [ICurrentStateAccess.cs                                                                                         ](src/YourCompany.OLTP.StateOwnership.Reflection.DI/ICurrentStateAccess.cs)</kbd><br>
<kbd>       + 9   |⁠ [ICurrentStateAccessProvider.cs                                                                                 ](src/YourCompany.OLTP.StateOwnership.Reflection.DI/ICurrentStateAccessProvider.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI ScopedRecordsProvider END -->

### Commit: 5|🧱: OLTP DI ScopedUseCasesProvider

<table><tbody><tr><td>

1 file changed, 69 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI/</sub></sub><br>
<kbd> +++++++ 69  |⁠ [ScopedUseCasesProvider.cs                                                                                      ](src/YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI/ScopedUseCasesProvider.cs)</kbd><br>

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI ScopedUseCasesProvider END -->

### Commit: 5|🧱: OLTP DI inheritance free repo

<table><tbody><tr><td>

8 files changed, 445 insertions(+), 1 deletion(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.DI/</sub></sub><br>
<kbd> +++++++ 164 |⁠ [RecordsBatchTransaction.CurrentStateAccess.cs                                                                  ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.CurrentStateAccess.cs)</kbd><br>
<kbd>     +++ 63  |⁠ [YourCompanyServiceCollectionExtensions.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement.DI/YourCompanyServiceCollectionExtensions.cs)</kbd><br>
<kbd>     +++ 62  |⁠ [ScopedRecordsBatchTransactionFactory.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.DI/ScopedRecordsBatchTransactionFactory.cs)</kbd><br>
<kbd>     +++ 56  |⁠ [RecordsBatchTransaction.CurrentStateAccess.Provider.cs                                                         ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.CurrentStateAccess.Provider.cs)</kbd><br>
<kbd>      ++ 52  |⁠ [RecordsBatchTransaction.CurrentlyResolvedRecord.cs                                                             ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.CurrentlyResolvedRecord.cs)</kbd><br>
<kbd>      ++ 35  |⁠ [ScopedRepository.cs                                                                                            ](src/YourCompany.OLTP.RecordsManagement.DI/ScopedRepository.cs)</kbd><br>
<kbd>       + 12  |⁠ [IRecordsDataAccessProvider.cs                                                                                  ](src/YourCompany.OLTP.RecordsManagement.DI/IRecordsDataAccessProvider.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [RecordsBatchTransaction.cs                                                                                     ](src/YourCompany.OLTP.RecordsManagement.DI/RecordsBatchTransaction.cs)</kbd><br>

You never need to implement it anymore since your record's  
(aggregate's) object creation is covered. The only required piece  
`IRecordsDataAccessProvider`. This entry point is good for putting  
first adapters in place as well as organizing full fledged ACID  
transaction based model with optional use case types:

- `IAuthorizer`
- `IChangesPreparer`
- `IFinishingHandler`
- `IExtraInterfacesProvider`

Just call `AddYourCompanyDomainTypes` service collection extension and  
you're all set. As of `RecordTypesMap` and `UseCaseTypesMap` you  
simply put your types in your application startup. If the type accepts  
`IStateAccess` or implements one of the supported use case types they  
are registered as scoped and will be used by a resolved IRepository.

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI inheritance free repo END -->

### Commit: 5|🧱: OLTP record composition to chassis

<table><tbody><tr><td>

17 files changed, 778 insertions(+), 771 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/</sub></sub><br>
<kbd> +++++++ 192 |⁠ [ComposableRecordsBatchTransaction.IteratingInParallel.cs                                                       ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.IteratingInParallel.cs)</kbd><br>
<kbd>   +++++ 152 |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.cs                                                     ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.cs)</kbd><br>
<kbd>   +++++ 149 |⁠ [ComposableRecordsBatchTransaction.ComposingRecords.cs                                                          ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ComposingRecords.cs)</kbd><br>
<kbd>    ++++ 116 |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs                                   ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs)</kbd><br>
<kbd>      ++ 50  |⁠ [AlwaysTrueSpecifications.cs                                                                                    ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/AlwaysTrueSpecifications.cs)</kbd><br>
<kbd>      ++ 48  |⁠ [ComposableRecordsBatchTransaction.IteratedInParallel.cs                                                        ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.IteratedInParallel.cs)</kbd><br>
<kbd>      ++ 41  |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs                                ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs)</kbd><br>
<kbd>       + 29  |⁠ [ComposableRecordsBatchTransaction.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.cs)</kbd><br>
<sub><sub><del>src/YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition/</del></sub></sub><br>
<kbd>  ‑‑‑‑‑‑ 187 |⁠ ComposableRecordsBatchTransaction.IteratingInParallel.cs                                                       </kbd><br>
<kbd>  ‑‑‑‑‑‑ 166 |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.cs                                                     </kbd><br>
<kbd>   ‑‑‑‑‑ 144 |⁠ ComposableRecordsBatchTransaction.ComposingRecords.cs                                                          </kbd><br>
<kbd>    ‑‑‑‑ 111 |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs                                   </kbd><br>
<kbd>      ‑‑ 54  |⁠ AlwaysTrueSpecifications.cs                                                                                    </kbd><br>
<kbd>      ‑‑ 43  |⁠ ComposableRecordsBatchTransaction.IteratedInParallel.cs                                                        </kbd><br>
<kbd>      ‑‑ 36  |⁠ ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs                                </kbd><br>
<kbd>       ‑ 29  |⁠ ComposableRecordsBatchTransaction.cs                                                                           </kbd><br>
<sub><sub>src/YourCompany.OLTP.StateOwnership.TransactionalComposition/</sub></sub><br>
<kbd>      +‑ 2   |⁠ [TransactionalCompositionTransactionCallback.cs                                                                 ](src/YourCompany.OLTP.StateOwnership.TransactionalComposition/TransactionalCompositionTransactionCallback.cs)</kbd><br>

We continue to internalize public classes and methods previously  
open for overriding per project and per record. The rationale may be  
found in the earlier commit.

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP record composition to chassis END -->

### Commit: 5|🧱: OLTP DI mixed repository

<table><tbody><tr><td>

14 files changed, 548 insertions(+), 32 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/</sub></sub><br>
<kbd> +++++++ 200 |⁠ [ComposingRecordsDataAccessProxy.cs                                                                             ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposingRecordsDataAccessProxy.cs)</kbd><br>
<kbd>     +++ 89  |⁠ [ScopedRepository.cs                                                                                            ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ScopedRepository.cs)</kbd><br>
<kbd>      ++ 64  |⁠ [ScopedRecordsBatchTransactionFactory.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ScopedRecordsBatchTransactionFactory.cs)</kbd><br>
<kbd>      +‑ 60  |⁠ [ComposableRecordsBatchTransaction.ComposingRecords.cs                                                          ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ComposingRecords.cs)</kbd><br>
<kbd>      ++ 54  |⁠ [YourCompanyServiceCollectionExtensions.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/YourCompanyServiceCollectionExtensions.cs)</kbd><br>
<kbd>      +‑ 44  |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs                                   ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.RepositoryRecords.cs)</kbd><br>
<kbd>      +‑ 24  |⁠ [ComposableRecordsBatchTransaction.IteratingInParallel.cs                                                       ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.IteratingInParallel.cs)</kbd><br>
<kbd>       + 12  |⁠ [IComposableRecordsDataAccess.cs                                                                                ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/IComposableRecordsDataAccess.cs)</kbd><br>
<kbd>       + 11  |⁠ [IComposableRecordsDataAccessProvider.cs                                                                        ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/IComposableRecordsDataAccessProvider.cs)</kbd><br>
<kbd>       + 9   |⁠ [IScopedUseCaseHandledEntityRecordTypeProvider.cs                                                               ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/IScopedUseCaseHandledEntityRecordTypeProvider.cs)</kbd><br>
<kbd>      +‑ 5   |⁠ [ComposableRecordsBatchTransaction.IteratedInParallel.cs                                                        ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.IteratedInParallel.cs)</kbd><br>
<kbd>      +‑ 3   |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs                                ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.NonRepositoryRecords.cs)</kbd><br>
<kbd>      +‑ 3   |⁠ [ComposableRecordsBatchTransaction.ConfiguredIdentically.cs                                                     ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.ConfiguredIdentically.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [ComposableRecordsBatchTransaction.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition/ComposableRecordsBatchTransaction.cs)</kbd><br>

Meet this neat <1K lines extension for the previously introduced  
inheritance free repository now adapting the domain types composition.

Just call `EnableYourCompanyDomainTypesTransactionalComposition`  
service collection extension and you're all set. As before you need to  
put your types within a corresponding map. From now on the resolved  
repository handles the relationships between the same-entity-related  
aggreagates.

Hence meet an extra convention to the previous regarded to  
`IStateAccess` accepting in the record type ctor. Now when your  
aggregate/record type accepts another one in the same single ctor - it  
becomes a mixin 😲 So the one without any such dependencies is  
"an entity" 😁, or to be more precise is an identity owner which must  
exist in your storage whenever you treat a particular entity instance  
registered in your system. The mixins in constrast may not have a  
record yet.

Worth to mention that your "entity types" still can have dependency on  
any cross cutting mixins without losing its "status" 😉

</td></tr></tbody></table>

<!-- ### Commit: 5|🧱: OLTP DI mixed repository END -->

### Commit: 5|💾: OLTP EFCore readonly

<table><tbody><tr><td>

21 files changed, 2256 insertions(+)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/</sub></sub><br>
<kbd> +++++++ 219 |⁠ [YourCompanyDbContextLockingRecordDataReader.cs                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.cs)</kbd><br>
<kbd>  ++++++ 194 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs                              ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs)</kbd><br>
<kbd>  ++++++ 184 |⁠ [YourCompanyDbContextLockingRecordDataReader.FactoriesCache.cs                                                  ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.FactoriesCache.cs)</kbd><br>
<kbd>   +++++ 182 |⁠ [YourCompanyDbContextRecordsDataAccessAdapter.cs                                                                ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextRecordsDataAccessAdapter.cs)</kbd><br>
<kbd>   +++++ 177 |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<kbd>   +++++ 166 |⁠ [YourCompanyDbContext.DelegatedPrimaryKeys.cs                                                                   ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.DelegatedPrimaryKeys.cs)</kbd><br>
<kbd>   +++++ 164 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs                                         ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs)</kbd><br>
<kbd>    ++++ 143 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs                                    ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs)</kbd><br>
<kbd>    ++++ 128 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs)</kbd><br>
<kbd>    ++++ 116 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs)</kbd><br>
<kbd>    ++++ 116 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs)</kbd><br>
<kbd>     +++ 94  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs                      ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs)</kbd><br>
<kbd>     +++ 92  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs                          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs)</kbd><br>
<kbd>      ++ 64  |⁠ [YourCompanyDbContext.cs                                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.cs)</kbd><br>
<kbd>      ++ 54  |⁠ [YourCompanyDbContext.QueryableRecordDataTypes.cs                                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.QueryableRecordDataTypes.cs)</kbd><br>
<kbd>       + 31  |⁠ [YourCompanyDbContext.DbContextLocking.cs                                                                       ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.DbContextLocking.cs)</kbd><br>
<kbd>       + 21  |⁠ [YourCompanyDbContextConfiguratorsLoadingContext.cs                                                             ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextConfiguratorsLoadingContext.cs)</kbd><br>
<kbd>       + 16  |⁠ [YourCompanyDbContextConfiguration.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextConfiguration.cs)</kbd><br>
<kbd>       + 14  |⁠ [YourCompanyDbContextConfigurator.cs                                                                            ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextConfigurator.cs)</kbd><br>
<kbd>       + 7   |⁠ [YourCompanyDbContextFactoryLoadingConfiguration.cs                                                             ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextFactoryLoadingConfiguration.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>     +++ 74  |⁠ [CollationAwareSortingUniqueKeyAdapter.cs                                                                       ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/CollationAwareSortingUniqueKeyAdapter.cs)</kbd><br>

This is a basic `ReadOnly` implementation of the records data access  
adapter. At the core is a semi-closed hierarchy of  
`YourCompanyDbContextLockingRecordDataReader` whose goal is to  
encapsulate differences of accessing by ids and without. Later we'll  
also introduce one for record data modifying part.

Here we also introduce `DbContext` locking contrary to the popular  
belief that its strictly single threaded. It **is** actually, and  
wherever you find logic decomposed, you'd see either  
"scoped" `DbContext` injected at several "handlers" limiting parallel  
processing completely or the "Unit-of-Work" limiting modularity  
completely. But we'll do it different, allowing `DbContext` to be  
locked by one of parallel conflicting tasks so that when it's one  
turn it can benefit of the `Local` view (local caching of previously  
accessed data, i.e. locked in the same transaction when previously  
modified) and the same time not bounding processing to a single  
`DbContext` instance as it's made when "scoped" (since we solely  
use DbContext factories).

</td></tr></tbody></table>

<!-- ### Commit: 5|💾: OLTP EFCore readonly END -->

### Commit: 6|💾: OLTP EFCore finalized

<table><tbody><tr><td>

19 files changed, 1146 insertions(+), 38 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/</sub></sub><br>
<kbd>  ++++++ 174 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Finishing.cs                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Finishing.cs)</kbd><br>
<kbd>  ++++++ 162 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.cs                           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.cs)</kbd><br>
<kbd>   +++++ 157 |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Modifying.cs                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Modifying.cs)</kbd><br>
<kbd>   ++++‑ 135 |⁠ [YourCompanyDbContextRecordsDataAccessAdapter.cs                                                                ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextRecordsDataAccessAdapter.cs)</kbd><br>
<kbd>    ++++ 104 |⁠ [YourCompanyDbContext.PessimisticLocking.cs                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.PessimisticLocking.cs)</kbd><br>
<kbd>    +++‑ 97  |⁠ [YourCompanyDbContext.DelegatedPrimaryKeys.cs                                                                   ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.DelegatedPrimaryKeys.cs)</kbd><br>
<kbd>     ++‑ 77  |⁠ [YourCompanyDbContextLockingRecordDataReader.cs                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.cs)</kbd><br>
<kbd>      +‑ 18  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs)</kbd><br>
<kbd>       + 10  |⁠ [YourCompanyDbContextConfiguration.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextConfiguration.cs)</kbd><br>
<kbd>       + 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs                                         ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs                      ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs                              ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs                          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs                                    ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs)</kbd><br>
<kbd>       + 1   |⁠ [YourCompanyDbContext.QueryableRecordDataTypes.cs                                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.QueryableRecordDataTypes.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd> +++++++ 191 |⁠ [EntityEntryModifyingVisitor.cs                                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/EntityEntryModifyingVisitor.cs)</kbd><br>
<kbd>      ++ 34  |⁠ [EntityEntryModifyingPair.cs                                                                                    ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/EntityEntryModifyingPair.cs)</kbd><br>

This is a modifying finalization for the previously implemented record  
data access adapter, adding an extra inheritor to the semi-closed  
hierarchy of `YourCompanyDbContextLockingRecordDataReader` whose  
responsibility is now slightly extended.

Worth mentioning that by extending its responsibility we're actually  
following the EFCore's ideology of **two DbCommand per one logical  
transaction** (i.e. read first and then change and save). This  
ideology may cost a lot in practice of high loaded apps and EFCore is  
often avoided for that reason. Why we still implement this? 🤔 To show  
**what layer it really belongs to** in the system as well as still  
benefit the change tracking capabilities to utilize it as smart ORM  
rather map changes to the "UPDATE setters" ourselves (btw, `linq2db`  
is a perfect candidate for the latter).

Furthermore, to not keep silent about this, we're switching to the  
pessimistic locking right away 😲 and by doing this we denote one of  
the major OLTP rules - **the transaction have to be fast and small**.  
We're not going to allow "to read first and then make sizeless logic"  
on the contrary we're deliberately bound by a **single repository call  
UoW** that at its worst allows to put extra transactional logic  
within its use case strategies, i.e. time and context bound.

</td></tr></tbody></table>

<!-- ### Commit: 6|💾: OLTP EFCore finalized END -->

### Commit: 7|🧱: OLTP DI EFCore switch to chassis

<table><tbody><tr><td>

51 files changed, 226 insertions(+), 221 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/</sub></sub><br>
<kbd> +++‑‑‑‑ 42  |⁠ [YourCompanyDbContextLockingRecordDataReader.cs                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.cs)</kbd><br>
<kbd>   ++‑‑‑ 30  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs                                         ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.cs)</kbd><br>
<kbd>     +‑‑ 16  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs                              ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.After.cs)</kbd><br>
<kbd>     +‑‑ 16  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs                          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.NoSorting.cs)</kbd><br>
<kbd>      +‑ 12  |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.cs                           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [YourCompanyDbContext.PessimisticLocking.cs                                                                     ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.PessimisticLocking.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<kbd>      +‑ 8   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Modifying.cs                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Modifying.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs                      ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Finishing.cs                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ReadBeforeModifying.Finishing.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs          ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromNatural.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs                                    ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.WithoutIds.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs           ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.SingleEntityQuery.ByIds.ResolvePrivateKeys.FromPublic.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContext.DelegatedPrimaryKeys.cs                                                                   ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.DelegatedPrimaryKeys.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContextRecordsDataAccessAdapter.cs                                                                ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextRecordsDataAccessAdapter.cs)</kbd><br>
<kbd>      +‑ 3   |⁠ [YourCompanyDbContext.cs                                                                                        ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContext.DbContextLocking.cs                                                                       ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.DbContextLocking.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextLockingRecordDataReader.FactoriesCache.cs                                                  ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextLockingRecordDataReader.FactoriesCache.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContext.QueryableRecordDataTypes.cs                                                               ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContext.QueryableRecordDataTypes.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/</sub></sub><br>
<kbd>     +‑‑ 16  |⁠ [EntityEntryPropertiesCopyingVisitor.NonGeneric.cs                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.NonGeneric.cs)</kbd><br>
<kbd>     +‑‑ 16  |⁠ [EntityEntryPropertiesCopyingVisitor.Generic.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.Generic.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [EntityEntryPropertiesSettingVisitor.NonGeneric.cs                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.NonGeneric.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [EntityEntrySortingKeyCreatingVisitor.Generic.cs                                                                ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.Generic.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [EntityEntryPropertiesSettingVisitor.Generic.cs                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.Generic.cs)</kbd><br>
<kbd>      +‑ 8   |⁠ [EntityEntrySortingKeyCreatingVisitor.NonGeneric.cs                                                             ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.NonGeneric.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [EntityEntrySortingKeyCreatingVisitor.cs                                                                        ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntrySortingKeyCreatingVisitor.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [EntityEntryPropertiesCopyingVisitor.cs                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesCopyingVisitor.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [EntityEntryPropertiesSettingVisitor.cs                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ChangeTracking/EntityEntryPropertiesSettingVisitor.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>    ++‑‑ 24  |⁠ [SortingKeyPropertiesVisitor.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyPropertiesVisitor.cs)</kbd><br>
<kbd>     +‑‑ 14  |⁠ [OrElseEqualityKeys.cs                                                                                          ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [SortingKey.Querying.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Querying.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [OrElseEqualityKeys.Querying.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.Querying.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [SortingKey.Visiting.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Visiting.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [ICollationCompatibleComparersProvider.cs                                                                       ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/ICollationCompatibleComparersProvider.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [OrElseEqualityKeys.ToString.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/OrElseEqualityKeys.ToString.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKeyPredicatesBuilder.cs                                                                                 ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKeyPredicatesBuilder.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKey.Building.cs                                                                                         ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.Building.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKey.ValueHolding.ConvertedFrom.cs                                                                       ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.ValueHolding.ConvertedFrom.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [SortingKey.ValueHolding.cs                                                                                     ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/SortingKey.ValueHolding.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/</sub></sub><br>
<kbd>     +‑‑ 16  |⁠ [EntityEntryModifyingVisitor.cs                                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/EntityEntryModifyingVisitor.cs)</kbd><br>
<kbd>      +‑ 10  |⁠ [EntityEntryModifyingPair.cs                                                                                    ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/CollationAwareSorting/EntityEntryModifyingPair.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/ChangeTracking/</sub></sub><br>
<kbd>     +‑‑ 14  |⁠ [EFChangeTrackerTrackGraphStrategy.cs                                                                           ](src/YourCompany.Configuration.EFCore/ChangeTracking/EFChangeTrackerTrackGraphStrategy.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [EFEntityEntryPropertiesCache.cs                                                                                ](src/YourCompany.Configuration.EFCore/ChangeTracking/EFEntityEntryPropertiesCache.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/ExpressionsCaching/</sub></sub><br>
<kbd>      +‑ 12  |⁠ [EFPropertyExpressionsCache.FromParameter.cs                                                                    ](src/YourCompany.Configuration.EFCore/ExpressionsCaching/EFPropertyExpressionsCache.FromParameter.cs)</kbd><br>
<kbd>      +‑ 8   |⁠ [EFPropertyExpressionsCache.cs                                                                                  ](src/YourCompany.Configuration.EFCore/ExpressionsCaching/EFPropertyExpressionsCache.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd>      +‑ 8   |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<kbd>      +‑ 6   |⁠ [YourCompanyDbContext.cs                                                                                        ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.cs)</kbd><br>
<kbd>      +‑ 4   |⁠ [YourCompanyDbContext.PessimisticLocking.cs                                                                     ](src/YourCompany.Configuration.EFCore/YourCompanyDbContext.PessimisticLocking.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/PessimisticLocking/</sub></sub><br>
<kbd>      +‑ 8   |⁠ [PessimisticLockingUpdateInterceptionContext.cs                                                                 ](src/YourCompany.Configuration.EFCore/PessimisticLocking/PessimisticLockingUpdateInterceptionContext.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/</sub></sub><br>
<kbd>      +‑ 2   |⁠ [EFEntityTypeSortingKeyTopology.cs                                                                              ](src/YourCompany.Configuration.EFCore/CollationAwareSorting/Metadata/EFEntityTypeSortingKeyTopology.cs)</kbd><br>

We continue to internalize public classes and methods previously  
open for overriding per project and for using extra capabilites  
implemented over original EFCore. The rationale may be found in the  
earlier commit.

</td></tr></tbody></table>

<!-- ### Commit: 7|🧱: OLTP DI EFCore switch to chassis END -->

### Commit: 7|🧱: OLTP DI EFCore integrated

<table><tbody><tr><td>

9 files changed, 323 insertions(+), 2 deletions(-)<br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.DI.EFCore/</sub></sub><br>
<kbd> +++++++ 136 |⁠ [YourCompanyServiceCollectionExtensions.cs                                                                      ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/YourCompanyServiceCollectionExtensions.cs)</kbd><br>
<kbd>   +++++ 102 |⁠ [ScopedYourCompanyDbContextFactory.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/ScopedYourCompanyDbContextFactory.cs)</kbd><br>
<kbd>     +++ 54  |⁠ [ScopedRecordsDataAccessProvider.cs                                                                             ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/ScopedRecordsDataAccessProvider.cs)</kbd><br>
<kbd>       + 14  |⁠ [YourCompanyDbContextConfigurator.cs                                                                            ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/YourCompanyDbContextConfigurator.cs)</kbd><br>
<kbd>       + 7   |⁠ [YourCompanyDbContextFactoryLoadingConfiguration.cs                                                             ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/YourCompanyDbContextFactoryLoadingConfiguration.cs)</kbd><br>
<kbd>       + 6   |⁠ [YourCompanyDbContextConfiguration.cs                                                                           ](src/YourCompany.OLTP.RecordsManagement.DI.EFCore/YourCompanyDbContextConfiguration.cs)</kbd><br>
<sub><sub>src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/</sub></sub><br>
<kbd>       + 2   |⁠ [YourCompanyDbContextRecordsDataAccessAdapter.cs                                                                ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextRecordsDataAccessAdapter.cs)</kbd><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>
<sub><sub>src/YourCompany.Configuration.EFCore/</sub></sub><br>
<kbd>      +‑ 2   |⁠ [YourCompanyDbContextFactory.cs                                                                                 ](src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs)</kbd><br>

This entry point benefits from the plugin based EFCore initialization  
letting to avoid manually building the record types map when you  
configure your domain types and repositories as well as allowing  
plugins to inject extra record data query builders both scoped and  
singleton.

As of the huge gap in lines with the previous entry point we can  
justify it by missing EFCore capabilities that were implemented in the  
merged libraries:

- properly unified `RunTime` and `DesignTime` `DbContext` factories  
working the same way both for queries and migrations for multiple  
switchable database providers;
- collation aware `SortingKey` unifying modeled columns DB side  
comparison with .NET in-memory comparers and providing advanced  
querying abilities;
- advanced abilities around `SortingKey` topology, allowing required  
change tracking strategies such as partial modifying by non-default  
properties set in the state and exposing public cursors while  
utilizing keyset pagination capabilities (`AfterId`);
- ofc imlementing the segregated record data access interfaces which  
are described in more details in the corresponding commits.

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
