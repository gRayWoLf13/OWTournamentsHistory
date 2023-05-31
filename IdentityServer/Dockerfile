#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR "/app"

COPY "IdentityServer" "/app/IdentityServer"

RUN dotnet restore "/app/IdentityServer/IdentityServer.csproj"
RUN dotnet build "/app/IdentityServer/IdentityServer.csproj" -c Release -o "/app/identity-server/build"
RUN dotnet publish "/app/IdentityServer/IdentityServer.csproj" -c Release -o "/app/identity-server/publish" /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

EXPOSE 5001

WORKDIR "/app"
COPY --from=base "/app/identity-server/publish" "/app/identity-server/publish"
ENTRYPOINT ["dotnet", "/app/identity-server/publish/IdentityServer.dll"]