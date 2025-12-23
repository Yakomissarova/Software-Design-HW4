# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем весь репозиторий (простая и надежная стратегия для учебного проекта)
COPY . .

# Restore + publish только хоста (он подтянет project references)
RUN dotnet restore "src/Orders_Service/Orders.Host/Orders.Host.csproj"
RUN dotnet publish "src/Orders_Service/Orders.Host/Orders.Host.csproj" -c Release -o /app/publish --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Слушаем внутри контейнера 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Orders.Host.dll"]
