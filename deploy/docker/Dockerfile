FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FileIngestion.Api/FileIngestion.Api.csproj", "FileIngestion.Api/"]
COPY ["src/FileIngestion.Application/FileIngestion.Application.csproj", "FileIngestion.Application/"]
COPY ["src/FileIngestion.Domain/FileIngestion.Domain.csproj", "FileIngestion.Domain/"]
COPY ["src/FileIngestion.Infrastructure/FileIngestion.Infrastructure.csproj", "FileIngestion.Infrastructure/"]
COPY ["src/FileIngestion.Shared/FileIngestion.Shared.csproj", "FileIngestion.Shared/"]
RUN dotnet restore "FileIngestion.Api/FileIngestion.Api.csproj"
COPY . .
WORKDIR "/src/FileIngestion.Api"
RUN dotnet build "FileIngestion.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileIngestion.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileIngestion.Api.dll"]
