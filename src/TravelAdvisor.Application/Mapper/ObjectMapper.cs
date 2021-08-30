
using System;
using System.Linq;
using AutoMapper;
using GoogleApi.Entities.Maps.Directions.Response;
using TravelAdvisor.Application.Models;

namespace TravelAdvisor.Application.Mapper
{
    // The best implementation of AutoMapper for class libraries -> https://www.abhith.net/blog/using-automapper-in-a-net-core-class-library/
    public static class ObjectMapper
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<TravelAdvisorModelMapper>();
            });
            var mapper = config.CreateMapper();
            return mapper;
        });
        public static IMapper Mapper => Lazy.Value;
    }

    public class TravelAdvisorModelMapper : Profile
    {
        public TravelAdvisorModelMapper()
        {
            OnMapperCreating();
        }
        
        public void CreateMap<TSource, TDestination>(Action<IMappingExpression<TSource, TDestination>> buildAction)
        {
            var mapping = CreateMap<TSource, TDestination>();
            buildAction.Invoke(mapping);
        }

        public void OnMapperCreating()
        {
            CreateMap<DirectionsResponse, Trip>(ConfigureTrip);
        }


        private void ConfigureTrip(IMappingExpression<DirectionsResponse, Trip> builder)
        {
            builder
                .ForMember(destination => destination.routes,
                    source => source.MapFrom(src => src.Routes));

        }

        private void ConfigureTravelInformation(IMappingExpression<DirectionsResponse, TravelInformation> builder)
        {
            builder
                .ForMember(destination => destination.distance,
                    source => source.MapFrom(src => src.Routes.First().Legs));

        }

    }
    
    
    
}