FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY ./src/TravelAdvisor.API/*.csproj /app/src/TravelAdvisor.API/
COPY ./src/TravelAdvisor.Application/*.csproj /app/src/TravelAdvisor.Application/
COPY ./src/TravelAdvisor.Domain/*.csproj /app/src/TravelAdvisor.Domain/
COPY ./src/TravelAdvisor.Infrastructure/*.csproj /app/src/TravelAdvisor.Infrastructure/

WORKDIR /app/src/TravelAdvisor.API/
RUN dotnet restore

COPY ./src /app/src
WORKDIR "/app/src/TravelAdvisor.API"
RUN dotnet build "TravelAdvisor.API.csproj" -c Release -o build

FROM build AS publish
RUN dotnet publish "TravelAdvisor.API.csproj" -c Release -o publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/src/TravelAdvisor.API/publish/ ./
ENTRYPOINT ["dotnet", "TravelAdvisor.API.dll"]