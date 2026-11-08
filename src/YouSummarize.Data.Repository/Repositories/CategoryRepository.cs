using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.Repositories;

public class CategoryRepository(IServiceProvider serviceProvider, IDbContextFactory<ApplicationDbContext> contextFactory) : ICategoryRepository
{
    public async Task<List<CategoryDto>> GetAll(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var categories = await context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
        return mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var category = await context.Categories.FindAsync([id], cancellationToken);
        return category == null ? null : mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> Add(CategoryDto category, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var categoryEntity = mapper.Map<CategoryEntity>(category);
        categoryEntity.CreatedDate = DateTime.UtcNow;

        await context.Categories.AddAsync(categoryEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        category.Id = categoryEntity.Id;
        category.CreatedDate = categoryEntity.CreatedDate;

        return category;
    }

    public async Task<bool> Update(CategoryDto category, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var categoryEntity = await context.Categories.FindAsync([category.Id], cancellationToken);

        if (categoryEntity == null)
        {
            return false;
        }

        categoryEntity.Name = category.Name;
        categoryEntity.Color = category.Color;

        context.Categories.Update(categoryEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var categoryEntity = await context.Categories.FindAsync([id], cancellationToken);

        if (categoryEntity == null)
        {
            return false;
        }

        // EF Core cascade delete will handle the removal of associations
        context.Categories.Remove(categoryEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IsUsedByAnySummary(int id, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.YoutubeSummarys
            .AnyAsync(y => y.Categories.Any(c => c.Id == id), cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetSummariesCountByCategory(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Categories
            .Select(c => new { c.Id, Count = c.YoutubeSummaries.Count })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);
    }
}
