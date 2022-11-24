FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SongList.Web/SongList.Web.csproj", "SongList.Web/"]
RUN dotnet restore "SongList.Web/SongList.Web.csproj"
COPY . .
WORKDIR "/src/SongList.Web"
RUN dotnet build "SongList.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SongList.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SongList.Web.dll"]
