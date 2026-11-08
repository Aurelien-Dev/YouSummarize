using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.Repositories;

public class TravelRepository(IServiceProvider serviceProvider, IDbContextFactory<ApplicationDbContext> contextFactory) : ITravelRepository
{
    public async Task<List<TravelDto>> GetAll(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var travels = await context.Travels.ToListAsync(cancellationToken);
        return mapper.Map<List<TravelDto>>(travels);
    }

    public async Task<TravelDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var travel = await context.Travels.FindAsync([id], cancellationToken);
        return travel == null ? null : mapper.Map<TravelDto>(travel);
    }

    public async Task<TravelDto> Add(TravelDto travel, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var travelEntity = mapper.Map<TravelEntity>(travel);
        travelEntity.CreatedDate = DateTime.UtcNow;

        await context.Travels.AddAsync(travelEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        travel.Id = travelEntity.Id;
        travel.CreatedDate = travelEntity.CreatedDate;

        return travel;
    }

    public async Task<bool> Update(TravelDto travel, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var travelEntity = await context.Travels.FindAsync([travel.Id], cancellationToken);

        if (travelEntity == null)
        {
            return false;
        }

        travelEntity.Name = travel.Name;
        travelEntity.StartDate = travel.StartDate;
        travelEntity.EndDate = travel.EndDate;
        travelEntity.IsArchived = travel.IsArchived;

        context.Travels.Update(travelEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var travelEntity = await context.Travels.FindAsync([id], cancellationToken);

        if (travelEntity == null)
        {
            return false;
        }

        context.Travels.Remove(travelEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Dictionary<int, int>> GetSummariesCountByTravel(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.YoutubeSummarys
            .Where(y => y.TravelId != null)
            .GroupBy(y => y.TravelId!.Value)
            .Select(g => new { TravelId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TravelId, x => x.Count, cancellationToken);
    }
}
