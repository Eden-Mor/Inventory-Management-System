# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER root

RUN apt update
RUN apt install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN apt-get install -y --no-install-recommends zlib1g fontconfig libfreetype6 libx11-6 libxext6 libxrender1 wget gdebi
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-3/wkhtmltox_0.12.6.1-3.bullseye_amd64.deb
RUN wget https://security.debian.org/debian-security/pool/updates/main/o/openssl/libssl1.1_1.1.1w-0+deb11u4_amd64.deb
RUN gdebi --n libssl1.1_1.1.1w-0+deb11u4_amd64.deb
RUN gdebi --n wkhtmltox_0.12.6.1-3.bullseye_amd64.deb
RUN ln -s /usr/local/lib/libwkhtmltox.so /usr/lib/libwkhtmltox.so


# This stage is used when running from VS in fast mode (Default for Debug configuration)
USER $APP_UID

WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IMS-Backend/IMS-Backend.csproj", "IMS-Backend/"]
COPY ["IMS-Shared/IMS-Shared.csproj", "IMS-Shared/"]
RUN dotnet restore "./IMS-Backend/IMS-Backend.csproj"
COPY . .
WORKDIR "/src/IMS-Backend"
RUN dotnet build "./IMS-Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IMS-Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY IMS-Backend/Templates ./Templates
ENTRYPOINT ["dotnet", "IMS-Backend.dll"]