using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using YouSummarize.Data;

using var shared = new DesignTimeDbContextFactory().CreateDbContext([]);
using var business = new YouSummarizeDesignTimeDbContextFactory().CreateDbContext([]);
var sharedModel = shared.GetService<IDesignTimeModel>().Model.GetRelationalModel();
var businessModel = business.GetService<IDesignTimeModel>().Model.GetRelationalModel();
string[] expectedTables =
[
    "AiConsumptions", "Categories", "DataProtectionKeys", "Prompts",
    "Travels", "YoutubeResumeCategories", "YoutubeSummarys"
];
Require(businessModel.Tables.Select(t => t.Name).Order().SequenceEqual(expectedTables.Order()),
    "Business model must contain exactly the seven business/key tables, with no Identity or recipes.");
string[] identityTables =
[
    "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims",
    "AspNetRoleClaims", "AspNetUserLogins", "AspNetUserTokens"
];
Require(identityTables.All(name => sharedModel.Tables.Any(t => t.Name == name)),
    "Shared context must retain all Identity tables.");

foreach (var table in businessModel.Tables)
{
    var existing = sharedModel.FindTable(table.Name, table.Schema)
        ?? throw new InvalidOperationException($"Missing source table {table.Name}.");
    Require(table.Columns.Select(ColumnShape).Order().SequenceEqual(existing.Columns.Select(ColumnShape).Order()),
        $"Column definitions changed for {table.Name}.");
    Require(table.PrimaryKey?.Columns.Select(c => c.Name).SequenceEqual(existing.PrimaryKey!.Columns.Select(c => c.Name)) == true,
        $"Primary key changed for {table.Name}.");
    Require(table.ForeignKeyConstraints.Select(ForeignKeyShape).Order()
        .SequenceEqual(existing.ForeignKeyConstraints.Select(ForeignKeyShape).Order()),
        $"Foreign keys changed for {table.Name}.");
    Require(table.Indexes.Select(IndexShape).Order().SequenceEqual(existing.Indexes.Select(IndexShape).Order()),
        $"Indexes changed for {table.Name}.");
}

var migrations = business.GetService<IMigrationsAssembly>();
Require(migrations.Migrations.Count == 1, "Exactly one initial business migration is expected.");
var migration = migrations.CreateMigration(migrations.Migrations.Single().Value, business.Database.ProviderName!);
Require(migration.UpOperations.OfType<CreateTableOperation>().Select(t => t.Name).Order()
    .SequenceEqual(expectedTables.Order()), "Initial migration must create business tables only.");
Require(migration.UpOperations.All(o => o is CreateTableOperation or CreateIndexOperation),
    "Initial migration must not modify or drop existing tables.");

var runtimeOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql("Host=localhost;Database=identity_guard_test;Username=design")
    .Options;
using var runtimeShared = new ApplicationDbContext(runtimeOptions);
foreach (var context in new[] { shared, runtimeShared })
{
    RequireGuarded(() => context.Database.Migrate());
    await RequireGuardedAsync(() => context.Database.MigrateAsync());
    RequireGuarded(() => context.GetService<IMigrator>().GenerateScript());
}

Require(business.GetService<IMigrator>().GenerateScript().Contains("CREATE TABLE \"Travels\"", StringComparison.Ordinal),
    "Business context must still generate its migration script.");

Console.WriteLine("PASS: Identity migration guard, business migrations and shared/business model compatibility.");
Console.WriteLine("No PostgreSQL connection was opened.");

static void RequireGuarded(Action action)
{
    try
    {
        action();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Migrations are disabled for ApplicationDbContext", StringComparison.Ordinal))
    {
        return;
    }

    throw new InvalidOperationException("Shared Identity migrations were not blocked.");
}

static async Task RequireGuardedAsync(Func<Task> action)
{
    try
    {
        await action();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Migrations are disabled for ApplicationDbContext", StringComparison.Ordinal))
    {
        return;
    }

    throw new InvalidOperationException("Async shared Identity migrations were not blocked.");
}

static string ColumnShape(IColumn c) => $"{c.Name}|{c.StoreType}|{c.IsNullable}|{c.DefaultValue}|{c.DefaultValueSql}";
static string ForeignKeyShape(IForeignKeyConstraint fk) =>
    $"{fk.Name}|{string.Join(',', fk.Columns.Select(c => c.Name))}|{fk.PrincipalTable.Name}|{string.Join(',', fk.PrincipalColumns.Select(c => c.Name))}|{fk.OnDeleteAction}";
static string IndexShape(ITableIndex index) =>
    $"{index.Name}|{index.IsUnique}|{string.Join(',', index.Columns.Select(c => c.Name))}";
static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
