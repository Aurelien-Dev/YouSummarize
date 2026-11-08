# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ===== Runtime (Alpine) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER root
# Client PostgreSQL 17 + SSH (pour les backups) + Node.js (pour yt-dlp challenge solver)
RUN apk add --no-cache postgresql17-client openssh-client nodejs
# repasser � l'utilisateur applicatif
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# �tape 2 : Build l�application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "./"]
COPY ["YouSummarize/YouSummarize.csproj", "YouSummarize/"]
COPY ["YouSummarize.Data/YouSummarize.Data.csproj", "YouSummarize.Data/"]
COPY ["YouSummarize.Dto/YouSummarize.Dto.csproj", "YouSummarize.Dto/"]
COPY ["YouSummarize.Data.Repository/YouSummarize.Data.Repository.csproj", "YouSummarize.Data.Repository/"]
RUN dotnet restore "./YouSummarize/YouSummarize.csproj"
COPY . .
WORKDIR "/src/YouSummarize"
RUN dotnet build "./YouSummarize.csproj" -c $BUILD_CONFIGURATION -o /app/build

# �tape 3 : Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YouSummarize.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# �tape 4 : Conteneur final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN chmod +x /app/Tools/yt-dlp/linux-x64/yt-dlp_musllinux
USER $APP_UID
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "YouSummarize.dll"]