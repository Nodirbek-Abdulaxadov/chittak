# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["NuGet.Config", "./"]
COPY ["src/server/Chittak.Server/Chittak.Server.csproj", "src/server/Chittak.Server/"]
COPY ["src/server/Chittak.Infrastructure/Chittak.Infrastructure.csproj", "src/server/Chittak.Infrastructure/"]
COPY ["src/server/Chittak.Application/Chittak.Application.csproj", "src/server/Chittak.Application/"]
COPY ["src/server/Chittak.Domain/Chittak.Domain.csproj", "src/server/Chittak.Domain/"]
RUN dotnet restore "./src/server/Chittak.Server/Chittak.Server.csproj"
COPY . .
WORKDIR "/src/src/server/Chittak.Server"
RUN dotnet build "./Chittak.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Chittak.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chittak.Server.dll"]