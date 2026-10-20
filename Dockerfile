FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/GameficandoEstudos.Domain/*.csproj src/GameficandoEstudos.Domain/
COPY src/GameficandoEstudos.Application/*.csproj src/GameficandoEstudos.Application/
COPY src/GameficandoEstudos.Infrastructure/*.csproj src/GameficandoEstudos.Infrastructure/
COPY src/GameficandoEstudos.Api/*.csproj src/GameficandoEstudos.Api/

RUN dotnet restore src/GameficandoEstudos.Api/GameficandoEstudos.Api.csproj

COPY src/ src/
RUN dotnet publish src/GameficandoEstudos.Api/GameficandoEstudos.Api.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["sh", "-c", "dotnet GameficandoEstudos.Api.dll --urls http://+:${PORT:-8080}"]
