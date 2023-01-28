# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build
COPY ./*.fsproj .
RUN dotnet restore
COPY . .
RUN dotnet build --configuration Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /build .
ENTRYPOINT ["/app/out/leaderboard-worker"]
