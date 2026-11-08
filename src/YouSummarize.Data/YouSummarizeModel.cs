using Microsoft.EntityFrameworkCore;
using YouSummarize.Data.EntityModel;

namespace YouSummarize.Data;

internal static class YouSummarizeModel
{
    internal static void Configure(ModelBuilder builder)
    {
        builder.Entity<PromptEntity>().HasKey(c => c.Id);

        builder.Entity<YoutubeResumeEntity>(entity =>
        {
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.Status).HasDefaultValue(SummaryStatus.ToReview);

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
    }
}
