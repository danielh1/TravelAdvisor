using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using TravelAdvisor.API.Middleware;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Logging;
using TravelAdvisor.Application.Services;

namespace TravelAdvisor.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<ExceptionHandlingMiddleware>();
            
            services.AddHttpClient();
            services.AddHttpClient<OpenWeatherMapHttpClient>((serviceProvider, client) =>
            {
               // client.BaseAddress = new Uri("http://localhost:5000");
            });
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelAdvisor.API", Version = "v1" });
                
                // Set the comments path for the Swagger JSON and UI.**
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath); 
            });
            
            services.AddScoped(typeof(IApplicatonLogger<>), typeof(LoggerAdapter<>));
            
            services.AddSingleton<IDirectionService, DirectionService>();
            services.AddSingleton<IGeocodeService, GeocodeService>();
            services.AddScoped<ITouringService, TouringService>();
            
            services.AddMemoryCache();
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                return ConnectionMultiplexer.Connect("travel-advisor-redis"); 
               //return ConnectionMultiplexer.Connect("localhost");
            });
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IClimateService, ClimateService>();
            services.AddScoped<IGetWeatherConditionsQuery, WeatherConditionsService>();
            
            services.AddAutoMapper(typeof(Startup)); // Add AutoMapper
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelAdvisor.API v1"));
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
