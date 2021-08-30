
# TravelAdvisor

A Sample WebApi project to demonstrate aggregation of data from GoogleMaps API and OpenWeatherMap API.

## Just want to try out this App ?

... Then update `../src/TravelAdvisor.API/appsettings.json` with your own API Keys

### Run ########
... and start the application:

```bash
docker-compose up --build
```

Then goto http://localhost:8080/swagger/index.html

### Rebuild ########
```bash
docker-compose up --build
```

## Requirements

- You will need to supply your own Google API key as well as a OpenWeatherMaps API key

1. Update `../src/TravelAdvisor.API/appsettings.json` with your own API Keys

```json
"GoogleApiKey" : "S6FBDxLcQPYfSB24MnXIfNw7X64F1roWyJk42Hu",
"OpenWeatherMapKey" : "Wj17lgJMLe5DT9X8243YI5cRKiFuGw3L",
``` 

2. This application requires a running Redis instance in order to cache http responses. You can use the `./TravelAdvisor/redis-docker-compose.yml` to launch a standalone Redis instance.


3. If you plan on working on this repo then you must update `../src/TravelAdvisor.API/Startup.cs` with your Redis Servername:

```csharp
 public void ConfigureServices(IServiceCollection services)
 {
    ...
    services.AddSingleton<IConnectionMultiplexer>(provider =>
    {
        return ConnectionMultiplexer.Connect("localhost");  // <--- change "localhost" to your own Redis server
    });
    ...
  }
```



