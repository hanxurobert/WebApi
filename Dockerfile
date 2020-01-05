FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY PornWebApi.csproj .
RUN dotnet restore "PornWebApi.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "PornWebApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PornWebApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PornWebApi.dll"]