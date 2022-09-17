FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Elastic.Kibana.Serilog/Elastic.Kibana.Serilog.csproj", "Elastic.Kibana.Serilog/"]
RUN dotnet restore "Elastic.Kibana.Serilog/Elastic.Kibana.Serilog.csproj"
COPY . .
WORKDIR "/src/Elastic.Kibana.Serilog"
RUN dotnet build "Elastic.Kibana.Serilog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elastic.Kibana.Serilog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Elastic.Kibana.Serilog.dll"]
