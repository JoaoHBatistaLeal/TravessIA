FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TravessIA.API/TravessIA.API.csproj", "TravessIA.API/"]
COPY ["TravessIA.Application/TravessIA.Application.csproj", "TravessIA.Application/"]
COPY ["TravessIA.Domain/TravessIA.Domain.csproj", "TravessIA.Domain/"]
COPY ["TravessIA.Infrastructure/TravessIA.Infrastructure.csproj", "TravessIA.Infrastructure/"]
RUN dotnet restore "TravessIA.API/TravessIA.API.csproj"
COPY . .
WORKDIR "/src/TravessIA.API"
RUN dotnet build "TravessIA.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TravessIA.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TravessIA.API.dll"]
