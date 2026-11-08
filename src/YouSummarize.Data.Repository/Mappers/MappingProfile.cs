using AutoMapper;
using YouSummarize.Data.EntityModel;
using YouSummarize.Dto.Models;


namespace YouSummarize.Data.Repository.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapping AI
        CreateMap<PromptEntity, PromptDto>().ReverseMap();
        CreateMap<AiConsumptionEntity, AiConsumptionDto>().ReverseMap();


        // Mapping Categories
        CreateMap<CategoryEntity, CategoryDto>().ReverseMap();

        // Mapping Entity → DTO (inclut TravelName et les catégories depuis les navigation properties)
        CreateMap<YoutubeResumeEntity, YoutubeResumeDto>()
            .ForMember(dest => dest.TravelName, opt => opt.MapFrom(src => src.Travel != null ? src.Travel.Name : null))
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.Categories.Select(c => c.Id).ToList()))
            .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.Name).ToList()))
            .ForMember(dest => dest.CategoryColors, opt => opt.MapFrom(src => src.Categories.Select(c => c.Color).ToList()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Dto.Models.SummaryStatus)src.Status))
            .ForMember(dest => dest.AiMetadataStatus, opt => opt.MapFrom(src => (Dto.Models.AiMetadataStatus)src.AiMetadataStatus));

        // Mapping DTO → Entity (ignore la navigation property, seul TravelId est mappé)
        CreateMap<YoutubeResumeDto, YoutubeResumeEntity>()
            .ForMember(dest => dest.Travel, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (EntityModel.SummaryStatus)src.Status))
            .ForMember(dest => dest.AiMetadataStatus, opt => opt.MapFrom(src => (EntityModel.AiMetadataStatus)src.AiMetadataStatus));

        CreateMap<TravelEntity, TravelDto>().ReverseMap();
    }
}