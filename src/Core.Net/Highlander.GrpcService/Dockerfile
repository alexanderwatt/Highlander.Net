#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Core.Net/Highlander.GrpcService/Highlander.GrpcService.csproj", "src/Core.Net/Highlander.GrpcService/"]
COPY ["src/Core.Net/Core.Common2/Highlander.Core.Common.csproj", "src/Core.Net/Core.Common2/"]
COPY ["src/Core.Net/Utilities/Highlander.Utilities.csproj", "src/Core.Net/Utilities/"]
RUN dotnet restore "src/Core.Net/Highlander.GrpcService/Highlander.GrpcService.csproj"
COPY . .
WORKDIR "/src/src/Core.Net/Highlander.GrpcService"
RUN dotnet build "Highlander.GrpcService.csproj" -c Release -o /app/build
RUN dotnet ef database update
WORKDIR /logs

FROM build AS publish
WORKDIR "/src/src/Core.Net/Highlander.GrpcService"
RUN dotnet publish "Highlander.GrpcService.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Highlander.GrpcService.dll"]