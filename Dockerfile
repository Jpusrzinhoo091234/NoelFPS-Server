# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copiar tudo e restaurar dependências
COPY . ./
RUN dotnet restore

# Buildar e publicar
RUN dotnet publish -c Release -o out

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Railway/Render usam a porta da variável de ambiente PORT
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "NoelFPS.Server.dll"]
