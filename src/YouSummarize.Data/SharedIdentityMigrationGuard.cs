using Microsoft.EntityFrameworkCore.Migrations;

namespace YouSummarize.Data;

internal sealed class SharedIdentityMigrationGuard : IMigrator
{
    private const string Message =
        "Migrations are disabled for ApplicationDbContext because it uses the shared Identity database.";

    public void Migrate(string? targetMigration = null) => throw new InvalidOperationException(Message);

    public Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        => throw new InvalidOperationException(Message);

    public bool HasPendingModelChanges() => throw new InvalidOperationException(Message);
}
