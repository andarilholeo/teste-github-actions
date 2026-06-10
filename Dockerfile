FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GeradorRelatorio.API/GeradorRelatorio.API.csproj GeradorRelatorio.API/
COPY GeradorRelatorio.Application/GeradorRelatorio.Application.csproj GeradorRelatorio.Application/
COPY GeradorRelatorio.Domain/GeradorRelatorio.Domain.csproj GeradorRelatorio.Domain/
COPY GeradorRelatorio.Infrastructure/GeradorRelatorio.Infrastructure.csproj GeradorRelatorio.Infrastructure/

RUN dotnet restore GeradorRelatorio.API/GeradorRelatorio.API.csproj

COPY . .

RUN dotnet publish GeradorRelatorio.API/GeradorRelatorio.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "GeradorRelatorio.API.dll"]