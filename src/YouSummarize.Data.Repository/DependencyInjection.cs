using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using YouSummarize.Data.Repository.IRepositories;
using YouSummarize.Data.Repository.Mappers;
using YouSummarize.Data.Repository.Repositories;

namespace YouSummarize.Data.Repository;

public static class DependencyInjection
{
    public static void AddRepository(this IServiceCollection services)
    {
        //Repositories
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IYoutubeRepository, YoutubeRepository>();
        services.AddScoped<ITravelRepository, TravelRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        //AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AddProfile<MappingProfile>();
        });
    }
}