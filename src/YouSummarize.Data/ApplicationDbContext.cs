using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.EntityModel.Blocks;
using System.Diagnostics;

namespace YouSummarize.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<RecipeEntity> Recipes { get; set; }
    public DbSet<AiConsumptionEntity> AiConsumptions { get; set; }
    public DbSet<PromptEntity> Prompts { get; set; }
    public DbSet<TagEntity> Tags { get; set; }

    public DbSet<YoutubeResumeEntity> YoutubeSummarys { get; set; }
    public DbSet<TravelEntity> Travels { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    // Data Protection keys table
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RecipeEntity>().HasKey(c => c.Id);
        builder.Entity<RecipeEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasMany(u => u.CreatedRecipes)
                  .WithOne(r => r.CreatedByUser)
                  .HasForeignKey(r => r.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(u => u.Favorites)
                  .WithMany(r => r.FavoritedByUsers)
                  .UsingEntity<Dictionary<string, object>>(
                      "UserFavoriteRecipes",
                      j => j.HasOne<RecipeEntity>()
                            .WithMany()
                            .HasForeignKey("RecipeId")
                            .HasPrincipalKey(nameof(RecipeEntity.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                      j => j.HasOne<ApplicationUser>()
                            .WithMany()
                            .HasForeignKey("ApplicationUserId")
                            .HasPrincipalKey(nameof(ApplicationUser.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                      j =>
                      {
                          j.HasKey("ApplicationUserId", "RecipeId");
                          j.ToTable("UserFavoriteRecipes");
                          j.HasIndex("RecipeId");
                      });
        });
        builder.Entity<BlockBaseEntity>().UseTpcMappingStrategy();

        builder.Entity<BlockBaseEntity>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.HasOne(c => c.Recipe)
                  .WithMany(c => c.BlocksInstructions)
                  .HasForeignKey(e => e.RecipeId)
                  .IsRequired();
        });

        builder.Entity<BlockImageEntity>(entity =>
        {

            entity.Property(p => p.Image).HasColumnType("BYTEA").IsRequired(false);
            entity.ToTable(t => t.HasCheckConstraint("CHK_Photo_ImageDataSize", $"octet_length(image_data) <= {1 * 1024 * 1024}")); // 1 Mo en bytes
        });

        builder.Entity<BlockIngredientListEntity>();
        builder.Entity<BlockInstructionEntity>();

        builder.Entity<IngredientEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.IngredientList)
                  .WithMany(e => e.Ingredients)
                  .HasForeignKey(e => e.IngredientListId);
        });

        builder.Entity<PromptEntity>().HasKey(c => c.Id);
        builder.Entity<BlockBaseEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Entity<YoutubeResumeEntity>(entity =>
        {
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.Status).HasDefaultValue(SummaryStatus.ToReview);

            // Many-to-many relationship with Categories
            entity.HasMany(y => y.Categories)
                  .WithMany(c => c.YoutubeSummaries)
                  .UsingEntity<Dictionary<string, object>>(
                      "YoutubeResumeCategories",
                      j => j.HasOne<CategoryEntity>()
                            .WithMany()
                            .HasForeignKey("CategoryId")
                            .HasPrincipalKey(nameof(CategoryEntity.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                      j => j.HasOne<YoutubeResumeEntity>()
                            .WithMany()
                            .HasForeignKey("YoutubeResumeId")
                            .HasPrincipalKey(nameof(YoutubeResumeEntity.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                      j =>
                      {
                          j.HasKey("YoutubeResumeId", "CategoryId");
                          j.ToTable("YoutubeResumeCategories");
                          j.HasIndex("CategoryId");
                      });
        });

        builder.Entity<CategoryEntity>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<TravelEntity>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.HasMany(t => t.YoutubeSummaries)
                  .WithOne(y => y.Travel)
                  .HasForeignKey(y => y.TravelId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(builder);
    }

    public static (bool, string, string) TriggerBackup()
    {
        try
        {
            var fileName = $"backup_{DateTime.UtcNow:s}.backup";
            var command = $"PGPASSWORD=PGUserPwd pg_dump -h recettesfamille.data -p 5432 -U pguser -d recettesfamilledb -F c -f wwwroot/backups/{fileName}";

            if (!Directory.Exists("wwwroot/backups"))
                Directory.CreateDirectory("wwwroot/backups");

            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process is null) throw new InvalidOperationException("Process not exist.");

            var output = process.StandardOutput.ReadToEnd();
            Console.WriteLine($"Output : {output}");

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Backup terminé avec succès : {fileName}");
                return (true, "Backup terminé avec succès : {fileName}", fileName);
            }
            else
                throw new InvalidOperationException($"Erreur lors du backup : {error}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception lors du backup : {ex.Message}");
            return (false, $"Exception lors du backup : {ex.Message}", string.Empty);
        }
    }
}


/// <summary>
/// Classe for EfCore Designer to enable dbContexte when create a new migration
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var postgresCs = "Host=recettes-db.atelier-cremazie.com;Port=5442;Database=test;Username=pguser;Password=PGUserPwd;Pooling=true";
        optionsBuilder.UseNpgsql(postgresCs);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
