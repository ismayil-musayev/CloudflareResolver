#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base

RUN apt update
RUN apt install -y chromium
RUN apt install -y xvfb
ENV DISPLAY=":99"

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CloudflareResolver/CloudflareResolver.csproj", "CloudflareResolver/"]
RUN dotnet restore "CloudflareResolver/CloudflareResolver.csproj"
COPY . .
WORKDIR "/src/CloudflareResolver"
RUN dotnet build "CloudflareResolver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CloudflareResolver.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudflareResolver.dll"]