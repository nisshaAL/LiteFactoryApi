FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LiteFactoryApi.csproj ./
RUN dotnet restore LiteFactoryApi.csproj

COPY . ./
RUN dotnet publish LiteFactoryApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "LiteFactoryApi.dll"]
