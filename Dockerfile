FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DidWeFeedTheCatToday/DidWeFeedTheCatToday.csproj", "./"]
RUN dotnet restore "DidWeFeedTheCatToday.csproj"

COPY . .
RUN dotnet build "DidWeFeedTheCatToday/DidWeFeedTheCatToday.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DidWeFeedTheCatToday/DidWeFeedTheCatToday.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DidWeFeedTheCatToday.dll"]