FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimpleTaskManager.csproj", "./"]
RUN dotnet restore "SimpleTaskManager.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SimpleTaskManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleTaskManager.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleTaskManager.dll"]
