# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy projects (Adjust paths if your folder names are different!)
COPY ["DidWeFeedTheCatToday.Shared/DidWeFeedTheCatToday.Shared.csproj", "DidWeFeedTheCatToday.Shared/"]
COPY ["DidWeFeedTheCatToday.Client/DidWeFeedTheCatToday.Client.csproj", "DidWeFeedTheCatToday.Client/"]
RUN dotnet restore "DidWeFeedTheCatToday.Client/DidWeFeedTheCatToday.Client.csproj"

COPY . .
WORKDIR "/src/DidWeFeedTheCatToday.Client"
RUN dotnet publish "DidWeFeedTheCatToday.Client.csproj" -c Release -o /app/publish

# STAGE 2: Run with Nginx
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY DidWeFeedTheCatToday.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80