# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution & projects
COPY *.sln .
COPY RibbitReels.Api/*.csproj RibbitReels.Api/
COPY RibbitReels.Data/*.csproj RibbitReels.Data/
COPY RibbitReels.Services/*.csproj RibbitReels.Services/
COPY RibbitReels.UnitTests/*.csproj RibbitReels.UnitTests/
COPY RibbitReels.IntegrationTests/*.csproj RibbitReels.IntegrationTests/
COPY RibbitReels.Benchmarks/*.csproj RibbitReels.Benchmarks/

# restore nuget packages
RUN dotnet restore

# copy all source files
COPY . .

# publish the api project
WORKDIR /src/RibbitReels.Api
RUN dotnet publish -c Release -o /app/publish

# final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RibbitReels.Api.dll"]
