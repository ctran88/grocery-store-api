FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["GroceryStoreAPI.csproj", "GroceryStoreAPI/"]
RUN dotnet restore "GroceryStoreAPI/GroceryStoreAPI.csproj"
WORKDIR "/src/GroceryStoreAPI"
COPY . .
RUN dotnet build "GroceryStoreAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GroceryStoreAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GroceryStoreAPI.dll"]
