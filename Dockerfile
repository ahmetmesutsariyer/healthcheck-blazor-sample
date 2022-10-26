FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Blazor.Server.App.Test/Blazor.Server.App.Test.csproj", "Blazor.Server.App.Test/"]
RUN dotnet restore "Blazor.Server.App.Test/Blazor.Server.App.Test.csproj"
COPY . .
WORKDIR "/src/Blazor.Server.App.Test"
RUN dotnet build "Blazor.Server.App.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blazor.Server.App.Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blazor.Server.App.Test.dll"]
