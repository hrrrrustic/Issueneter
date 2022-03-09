FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["./Issueneter.WebAPI/", "./Issueneter.WebAPI/src/"]
RUN dotnet restore "./Issueneter.WebAPI/src/Issueneter.WebAPI.fsproj"

COPY . .
WORKDIR "/src/Issueneter.WebAPI"
RUN dotnet build "Issueneter.WebAPI.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Issueneter.WebAPI.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Issueneter.WebAPI.dll"]
