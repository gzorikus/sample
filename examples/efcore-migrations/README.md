# EFCore migrations

## Commit: 2|💾: basic pluggable EFCore

### Install EFCore Tools

```powershell
dotnet tool install --global dotnet-ef --version 9.*
```

### Select provider

Proceed to [appsettings.json](src/YourCompany.ConsoleProgram/appsettings.json) to switch between Sqlite and PostgreSQL.
You can have all the providers configured simultaneously  
but make sure only one provider plugin `dll` is present in `PluginPaths` since in the current implementation the plugins do  
not distinguish DbContext (not recommended, though you'd make it easily configurable, e.g. by `DbContextConfigurationType`).

> Note: this is not neccessary to have the provider plugins directly referenced in
[YourCompany.ConsoleProgram.csproj](src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj). You'd  
comment these in `.csproj` and uncomment the corresponding in `PluginPaths` from
[appsettings.json](src/YourCompany.ConsoleProgram/appsettings.json). The current  
direct reference is made just for simplicity to not organize the binaries deployment here (this is up to your DevOps 😉).

#### PostgreSQL tips

To configure your fresh server installation corresponding with [Npgsql](src/YourCompany.ConsoleProgram/appsettings.json)
connection string add the following lines to `pg_hba.conf`  
under the instance installation directory (otherwise you'd need to put a password at the connection string):

```conf
# IPv4 local connections:
host    YourCompanyDemoDatabase     YourCompanyDemoUser     127.0.0.1/32    trust
# IPv6 local connections:
host    YourCompanyDemoDatabase     YourCompanyDemoUser     ::1/128         trust
```

Next reload the config: `pg_ctl reload -D path/to/data/dir`.

### Migrations from scratch

To practice, you first need to remove currently committed migrations at a chosen provider's assembly. But the options may be confusing:

* `YourCompany.EFCore.PostgreSQL.CustomConfigurationMigrations`
* `YourCompany.EFCore.PostgreSQL.YourCompanyDbContextConfigurationMigrations`
* `YourCompany.EFCore.Sqlite.CustomConfigurationMigrations`
* `YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations`

> That might be much easier to have only one assembly per provider for our demo purposes using "--output-dir" option, but...  
The surprize is the latter doesn't support generics 😢. I.e. when creating `*Snaphot.cs` it will respect only the context's  
own name part wihout its "configuration type" leading to overwriting the `*ModelSnapshot` at the wrong sibling subfolder.

So instead of separating by subfolders with "--output-dir" we put the configuration name as a suffix to a distinct  
assembly per configuration. Not ideal but enough. If you ever need it, though not encouraged, you may want to extract the  
migrations assembly name to the providers' plugins config accessed via
[YourCompanyDbContextConfiguratorsLoadingContext](../../src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfiguratorsLoadingContext.cs)'s  
base class by [NpgsqlDbContextConfigurator](../../src//YourCompany.Configuration.EFCore.PostgreSQL/NpgsqlDbContextConfigurator.cs)
and [SqliteDbContextConfigurator](../../src/YourCompany.Configuration.EFCore.Sqlite/SqliteDbContextConfigurator.cs).

Among the options there are 2 `YourCompanyDbContext` configurations:

* `CustomConfiguration` - this demonstrates how to have an extra context registered (requiring to define  
[EFCoreCustomConfigurationDesignTimeFactory](src/YourCompany.ConsoleProgram/EFCoreCustomConfigurationDesignTimeFactory.cs)
strictly at the entry assembly, i.e. specified in "--startup-project" below);
* `YourCompanyDbContextConfiguration` - the default design time factory is used for this (see
[DefaultDesignTimeForMigrations](../../src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs))  
when you don't need to extend
[YourCompanyDbContextConfiguration](../../src/YourCompany.Configuration.EFCore/YourCompanyDbContextConfiguration.cs) with any extra options.

#### Kickstart for the first time

**There is a trick** needed when you're going to ever reuse your DbContext (in case you aren't into inheriting it per each project what  
we actually discourage here). By default EF Tools are only looking for contexts (and
[`*DesignTime*`](../../src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs) factories) at  
"--startup-project" or "--project", but the reusable context is defined at your framework level and can't be seen.

For `CustomConfiguration` not only EF Tools are incapable to discover contexts in referenced assemblies it's also incapable to match  
an apropriate [`*DesignTime*`](../../src/YourCompany.Configuration.EFCore/YourCompanyDbContextFactory.cs) factory with open generics.
That's why we defined
[EFCoreCustomConfigurationDesignTimeFactory](src/YourCompany.ConsoleProgram/EFCoreCustomConfigurationDesignTimeFactory.cs) inheritor  
and since it's located in "--startup-project" EF Tools is able to see the context with less effort:

```powershell
dotnet ef dbcontext list `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
# YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.EFCore.CustomConfiguration, YourCompany.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
```

> Briefly about "--override" arguments, this is for demonstrating how your developers can share the same environment.  
More details you can find in [EnvironmentConventions](../../src/YourCompany.Configuration/EnvironmentConventions.cs).
Note also that for Production you **never commit the prefixed objects**.  
When you switch the branch you'd need to add a temporary migration to populate all the objects at once. And before that  
you'd also need to remove the old migrations and `*ModelSnapshot` files to avoid conflicts with the previous model. Once you  
prepare the branch for publishing you'd revert/reset the previous files and regenerate your migration without any prefixes.

> P.S. If you run this example under the exact commit rather than one of supporting entry points make sure to create and  
checkout a temporary `configuration-efcore` branch locally.

As for the default `YourCompanyDbContextConfiguration` this is a bit different:

```powershell
dotnet ef dbcontext list `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
# YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.EFCore.CustomConfiguration, YourCompany.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
# YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.KickStartDbContext
```

As you can see it yields the context for `CustomConfiguration` since it was defined in "--startup-project" but not  
`YourCompanyDbContext<YourCompanyDbContextConfiguration>` that we expect.

**The trick** is to teach EF Tools to see the desired context that can actually be placed anywhere. For that we first  
kickstart migrations by a dummy context:

```powershell
dotnet ef migrations add 'InitMigrations' -o 'KickStartMigrations' `
    -c 'YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.KickStartDbContext' `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
```

Then simply copy or move the generated migration and swich it to your desired context configuration.

> Don't forget to update the namespace and to change its `DbContext` attribute to the desired context!

Once finished you won't need
[KickStartDbContext.cs](src/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations/KickStartDbContext.cs)
and `KickStartMigrations` directory anymore so they're safe to be deleted.

> But make sure you copy the `*InitMigration` to `CustomConfigurationMigrations` as well! Otherwise you'll see another  
strange behavior from EF Tools regarding the `*ModelSnapshot`.

**In essence** we just need the context to be referenced anywere in "--startup-project" or "--project" assemblies to become discoverable.

After doing that trick you'll see the referenced context in the `dbcontext list` command's output:

```powershell
# YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.EFCore.CustomConfiguration, YourCompany.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
# YourCompany.EFCore.Sqlite.Migrations.YourCompanyDbContextConfiguration.KickStartDbContext
# YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.Configuration.EFCore.YourCompanyDbContextConfiguration, YourCompany.Configuration.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
```

Now any of these contexts can be specified to further manage corresponding migrations.

#### Add migration

For **default** context configuration:

```powershell
dotnet ef migrations add 'ItsAlive' -o '.' `
    -c 'YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.Configuration.EFCore.YourCompanyDbContextConfiguration, YourCompany.Configuration.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]' `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
```

For **custom** context configuration:

```powershell
dotnet ef migrations add 'ItsAlive' -o '.' `
    -c 'YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.EFCore.CustomConfiguration, YourCompany.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]' `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
```

> You can switch to another provider simply by changing "--project" and the line in
[PluginPaths](src/YourCompany.ConsoleProgram/appsettings.json).

#### Run pending migrations

For **default** context configuration:

```powershell
dotnet ef database update `
    -c 'YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.Configuration.EFCore.YourCompanyDbContextConfiguration, YourCompany.Configuration.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]' `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations/YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
```

For **custom** context configuration:

```powershell
dotnet ef database update `
    -c 'YourCompany.Configuration.EFCore.YourCompanyDbContext`1[[YourCompany.EFCore.CustomConfiguration, YourCompany.EFCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]' `
    --startup-project 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/YourCompany.ConsoleProgram.csproj' `
    --project 'examples/efcore-migrations/src/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations/YourCompany.EFCore.Sqlite.CustomConfigurationMigrations.csproj' `
    -- `
    --override YOURCOMPANY_ENVIRONMENT='Development' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
    --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
```

> You can switch to another provider simply by changing "--project" and the line in
[PluginPaths](src/YourCompany.ConsoleProgram/appsettings.json).

#### Bonus: check Sqlite created tables after migrations run

```powershell
sqlite3 'examples/efcore-migrations/src/YourCompany.ConsoleProgram/bin/Debug/net8.0/path-to-file.db' '.tables'
```

<!-- ## Commit: 2|💾: basic pluggable EFCore END -->

## Commit: 2|💾: EFCore hosting migration run

Meet `MigrateDatabaseOnStart` option in [appsettings.json](src/YourCompany.ConsoleProgram/appsettings.json)
allowing to avoid manual [pending migrations run](#run-pending-migrations) what can be useful  
especially for **Development** environment when you might have to switch between branches pretty often.

> Thanks to these arguments that happens quick and painless, because your branches won't conflict each other anymore:
> 
> ```powershell
>     -- `
>     --override YOURCOMPANY_ENVIRONMENT='Development' `
>     --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX='examples-' `
>     --override YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH=.
> ```

> P.S. If you run this example under the exact commit rather than one of supporting entry points make sure to create and  
checkout a temporary `configuration-efcore-hosting-migration-run` branch locally.

<!-- ## Commit: 2|💾: EFCore hosting migration run END -->

<!-- ## Commit: 7|🧱: OLTP DI EFCore integrated -->
