FROM mcr.microsoft.com/dotnet/sdk:8.0 as backend

WORKDIR src

RUN apt-get update \
    && apt-get install -y --no-install-recommends openjdk-17-jdk-headless \
    && rm -rf /var/lib/apt/lists/*

COPY ./*.sln ./
COPY ./**/*.csproj ./

RUN for f in *.csproj; do \
        filename=$(basename $f) && \
        dirname=${filename%.*} && \
        mkdir $dirname && \
        mv $filename ./$dirname/; \
    done
RUN dotnet restore SongList.Web/SongList.Web.csproj

COPY ./ ./
RUN dotnet publish SongList.Web/SongList.Web.csproj --output ./publish
RUN mkdir -p ./publish/holyrics-sync-helper/build \
    && cp -r ./SongList.Holyrics/holyrics-sync-helper/build/* ./publish/holyrics-sync-helper/build/

FROM node:lts-alpine as frontend 
ARG BOT_USERNAME
ENV VITE_BOT_USERNAME=${BOT_USERNAME}
WORKDIR app 

COPY frontend/package.json .
RUN npm install 

COPY frontend . 
RUN npm run build 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as final 
WORKDIR app 
RUN apt-get update \
    && apt-get install -y --no-install-recommends openjdk-17-jre-headless \
    && rm -rf /var/lib/apt/lists/*
COPY --from=backend src/publish .
COPY --from=frontend app/dist wwwroot

EXPOSE 80
ENV TZ="Europe/Moscow"
ENV ASPNETCORE_URLS http://0.0.0.0:80
ENTRYPOINT ["dotnet", "SongList.Web.dll"]
