#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR "/app"

COPY "OWTournamentsHistory.Api" "/app/Api/OWTournamentsHistory.Api/"
COPY "OWTournamentsHistory.Contract" "/app/Api/OWTournamentsHistory.Contract/"
COPY "OWTournamentsHistory.DataAccess" "/app/Api/OWTournamentsHistory.DataAccess/"

RUN dotnet restore "/app/Api/OWTournamentsHistory.Api/OWTournamentsHistory.Api.csproj"
RUN dotnet build "/app/Api/OWTournamentsHistory.Api/OWTournamentsHistory.Api.csproj" -c Release -o "/app/build"
RUN dotnet publish "/app/Api/OWTournamentsHistory.Api/OWTournamentsHistory.Api.csproj" -c Release -o "/app/publish" /p:UseAppHost=false

COPY "OWTournamentsHistory.Api/docker-entrypoint.sh" "/app/publish/"

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

EXPOSE 80
EXPOSE 443

WORKDIR "/app"
COPY --from=base "/app/publish" "/app/publish"
ENTRYPOINT "/bin/bash" -c "/app/publish/docker-entrypoint.sh"