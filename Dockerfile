FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY *.sln .
COPY ManagementPortal/Client/ManagementPortal.Client.csproj ./ManagementPortal/Client/
COPY ManagementPortal/Server/ManagementPortal.Server.csproj ./ManagementPortal/Server/
COPY ManagementPortal/Shared/ManagementPortal.Shared.csproj ./ManagementPortal/Shared/

RUN dotnet restore
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
RUN apt-get install -y nodejs


COPY . .
WORKDIR /source/ManagementPortal/Server/
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ManagementPortal.Server.dll", "migrate"]