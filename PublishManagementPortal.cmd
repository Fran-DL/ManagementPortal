@REM  Script para lanzar el proyecto. 
@echo off
setlocal

set ASPNETCORE_ENVIRONMENT=Development
set PORT=7109
tar -xf "ManagementPortal_SC.zip"

cd "ManagmentPortal.Client_selfcontained"


echo Running the application with environment: %ASPNETCORE_ENVIRONMENT% and port: %PORT%...
echo URL del servidor: "https://localhost:%PORT%"
dotnet ManagementPortal.Server.dll migrate --urls "https://localhost:%PORT%"

endlocal