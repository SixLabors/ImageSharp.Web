FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# submodules
## TODO Make the modules work with .gitmodules
WORKDIR /app
RUN  git clone https://github.com/SixLabors/SharedInfrastructure shared-infrastructure


# csproj ImageSharp.Web.Docker
WORKDIR /app/src/ImageSharp.Web.Docker
COPY src/ImageSharp.Web.Docker/ImageSharp.Web.Docker.csproj .
RUN dotnet restore ImageSharp.Web.Docker.csproj

# csproj ImageSharp.Web
WORKDIR /app/src/ImageSharp.Web
COPY src/ImageSharp.Web/ImageSharp.Web.csproj .
RUN dotnet restore ImageSharp.Web.csproj

# copy
COPY src/ImageSharp.Web.Docker /app/src/ImageSharp.Web.Docker
COPY src/ImageSharp.Web /app/src/ImageSharp.Web
COPY SixLabors.ImageSharp.Web.props /app

# build
WORKDIR /app/src/ImageSharp.Web.Docker
ARG Configuration=Release
RUN dotnet build ImageSharp.Web.Docker.csproj -c $Configuration -o /app/build

FROM build AS publish
ARG Configuration=Release
RUN dotnet publish ImageSharp.Web.Docker.csproj -c $Configuration -o /app/publish -f net6.0 /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageSharp.Web.Docker.dll"]
