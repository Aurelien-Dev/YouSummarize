using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using YouSummarize.Data.EntityModel;

namespace YouSummarize.Data;

public class YouSummarizeDbContext(DbContextOptions<YouSummarizeDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<AiConsumptionEntity> AiConsumptions { get; set; }
    public DbSet<PromptEntity> Prompts { get; set; }
    public DbSet<YoutubeResumeEntity> YoutubeSummarys { get; set; }
    public DbSet<TravelEntity> Travels { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        YouSummarizeModel.Configure(builder);
        base.OnModelCreating(builder);
    }
}

public class YouSummarizeDesignTimeDbContextFactory : IDesignTimeDbContextFactory<YouSummarizeDbContext>
{
    public YouSummarizeDbContext CreateDbContext(string[] args)
    {
        // Design-time commands generate/compare models only; never use production credentials.
        var options = new DbContextOptionsBuilder<YouSummarizeDbContext>()
            .UseNpgsql("Host=localhost;Database=yousummarize_design;Username=design")
            .Options;
        return new YouSummarizeDbContext(options);
    }
}
