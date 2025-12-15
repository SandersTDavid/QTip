# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (better Docker caching)
COPY src/QTip.Api/QTip.Api.csproj src/QTip.Api/
COPY src/QTip.Domain/QTip.Domain.csproj src/QTip.Domain/
COPY src/QTip.Infrastructure/QTip.Infrastructure.csproj src/QTip.Infrastructure/

RUN dotnet restore src/QTip.Api/QTip.Api.csproj

# Copy the rest
COPY . .

RUN dotnet publish src/QTip.Api/QTip.Api.csproj -c Release -o /app/publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QTip.Api.dll"]
