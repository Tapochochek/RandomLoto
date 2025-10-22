# ---------- Этап сборки ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Копируем всё решение из корня проекта (контекст будет .)
COPY RandomTrust.sln ./
COPY src ./src

WORKDIR /app/src/RandomTrust.Web
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# ---------- Этап выполнения ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "RandomTrust.Web.dll"]
