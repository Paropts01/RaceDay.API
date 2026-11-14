# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/RaceDay.API"
RUN dotnet publish "RaceDay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the lightweight runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (standard for ASP.NET Core in Linux containers)
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]
