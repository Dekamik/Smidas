FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Smidas.API/Smidas.API.csproj", "Smidas.API/"]
RUN dotnet restore "Smidas.API/Smidas.API.csproj"
COPY . .
WORKDIR "/src/Smidas.API"
RUN dotnet build "Smidas.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Smidas.API.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smidas.API.dll"]
