FROM mcr.microsoft.com/dotnet/sdk:8.0 as backend

WORKDIR app

COPY SongList.Web/SongList.Web.csproj .
RUN dotnet restore SongList.Web.csproj

COPY SongList.Web .
RUN dotnet publish SongList.Web.csproj --output ./publish

FROM node:lts-alpine as frontend 

WORKDIR app 

COPY frontend/package.json .
RUN npm install 

COPY frontend . 
RUN npm run build 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as final 
WORKDIR app 
COPY --from=backend app/publish .
COPY --from=frontend app/dist wwwroot

EXPOSE 80
ENV ASPNETCORE_URLS http://0.0.0.0:80
ENTRYPOINT ["dotnet", "SongList.Web.dll"]