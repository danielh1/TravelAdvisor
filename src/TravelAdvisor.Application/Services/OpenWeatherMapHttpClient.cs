
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TravelAdvisor.Application.Interfaces;
using TravelAdvisor.Application.Models;


namespace TravelAdvisor.Application.Services
{
    public class OpenWeatherMapHttpClient
    {
        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            PropertyNameCaseInsensitive = true
        };
        
       private readonly IApplicatonLogger<OpenWeatherMapHttpClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string apiKey; 

        public OpenWeatherMapHttpClient(IConfiguration configuration, HttpClient httpClient, IApplicatonLogger<OpenWeatherMapHttpClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            apiKey = _configuration["OpenWeatherMapKey"];
        }

        public async Task<OpenWeather> GetForecast(double lat, double lng)
        {
            
            var APIURL =  $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid={apiKey}&units=metric";
            using var request = new HttpRequestMessage(HttpMethod.Get, APIURL);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();
            
            await using var responseContent = await response.Content.ReadAsStreamAsync();
            _logger.LogInformation($"responseContent [{lat}, {lng}] = {response.Content.ReadAsStringAsync()}");
            //response.Content.ReadAsStringAsync().Result

            return  JsonConvert.DeserializeObject<OpenWeather>(await response.Content.ReadAsStringAsync());
            
            //await using var responseContent = await response.Content.ReadAsStreamAsync();
            //return await JsonSerializer.DeserializeAsync<OpenWeather>(responseContent, DefaultJsonSerializerOptions);
        }
    }
}