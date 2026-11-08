using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.Repositories;

public class YoutubeRepository(IServiceProvider serviceProvider, IDbContextFactory<ApplicationDbContext> contextFactory) : IYoutubeRepository
{
    public async Task<List<YoutubeResumeDto>> GetAllSummary(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var summaries = await context.YoutubeSummarys
            .Include(y => y.Travel)
            .Include(y => y.Categories)
            .ToListAsync(cancellationToken);
        return mapper.Map<List<YoutubeResumeDto>>(summaries);
    }

    public async Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = mapper.Map<YoutubeResumeEntity>(youtubeSummary);

        // Charger les catégories depuis la base de données si des CategoryIds sont fournis
        if (youtubeSummary.CategoryIds != null && youtubeSummary.CategoryIds.Count > 0)
        {
            var categories = await context.Categories
                .Where(c => youtubeSummary.CategoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            youtubeResumeEntity.Categories = categories;
        }

        await context.YoutubeSummarys.AddAsync(youtubeResumeEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);

        return youtubeSummary;
    }

    public async Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([id], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        context.YoutubeSummarys.Remove(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        youtubeResumeEntity.TravelId = travelId;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateStatus(int summaryId, Dto.Models.SummaryStatus status, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        // Convert DTO enum to Entity enum (they have the same values)
        youtubeResumeEntity.Status = (EntityModel.SummaryStatus)status;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys
            .Include(y => y.Categories)
            .FirstOrDefaultAsync(y => y.Id == summaryId, cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        // Load the categories from the database
        var categories = await context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        // Clear existing categories and add new ones
        youtubeResumeEntity.Categories.Clear();
        foreach (var category in categories)
        {
            youtubeResumeEntity.Categories.Add(category);
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        youtubeResumeEntity.Title = title;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
