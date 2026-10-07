FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Pata.slnx ./
COPY Pata.Domain/Pata.Domain.csproj Pata.Domain/
COPY Pata.Application/Pata.Application.csproj Pata.Application/
COPY Pata.Infrastructure/Pata.Infrastructure.csproj Pata.Infrastructure/
COPY Pata.Api/Pata.Api.csproj Pata.Api/
RUN dotnet restore Pata.Api/Pata.Api.csproj
COPY . .
RUN dotnet publish Pata.Api/Pata.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pata.Api.dll"]
