using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YouSummarize.Data.EntityModel;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Dto.Models;

namespace YouSummarize.Data.Repository.Repositories;

public class AiRepository(IMapper mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : IAiRepository
{

    public async Task<PromptDto> GetPrompt(string promptName, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await context.Prompts.Where(r => r.Name == promptName).FirstOrDefaultAsync(cancellationToken);

        return mapper.Map<PromptDto>(result);
    }

    public async Task<List<PromptDto>> GetPrompt(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await context.Prompts.ToListAsync(cancellationToken);

        return mapper.Map<List<PromptDto>>(result);
    }

    public async Task UpdatePrompt(PromptDto? prompt, CancellationToken cancellationToken = default)
    {
        if (prompt is null)
            return;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<PromptEntity>().FindAsync([prompt.Id], cancellationToken);

        mapper.Map(prompt, element);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AddPrompt(PromptDto? prompt, CancellationToken cancellationToken = default)
    {
        if (prompt is null)
            return false;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var promptEntity = mapper.Map<PromptEntity>(prompt);

        await context.AddAsync(promptEntity, cancellationToken);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task ReportConsumption(AiConsumptionDto aiConsumptionDto, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = mapper.Map<AiConsumptionEntity>(aiConsumptionDto);
        await context.AddAsync(element, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }



}
