@echo off
setlocal

set SOLUTION_FILE=ManagementPortal.sln
set PROJECT_SERVER=ManagementPortal/Server/ManagementPortal.Server.csproj
set OUTPUT_DIR=bin\Release\net8.0\publish

if "%1"=="" (
    echo No se de indico ambiente, se estable Desarrollo por defecto.
    set ASPNETCORE_ENVIRONMENT=Development
) else (
    set ASPNETCORE_ENVIRONMENT=%1
)

if "%2"=="" (
    echo No se indico puerto, se estable 7109 por defecto.
    set PORT=7109
) else (
    set PORT=%2
)

echo Using environment: %ASPNETCORE_ENVIRONMENT%
echo Using port: %PORT%

if exist %OUTPUT_DIR% (
    rmdir /s /q %OUTPUT_DIR%
)

echo Restoring packages...
dotnet restore %SOLUTION_FILE%

echo Building the project...
dotnet build %SOLUTION_FILE% -c Release

echo Publishing the project...
dotnet publish %PROJECT_SERVER% -c Release -o %OUTPUT_DIR%

cd %OUTPUT_DIR%

set ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT%

echo Running the application with environment: %ASPNETCORE_ENVIRONMENT% and port: %PORT%...
dotnet ManagementPortal.Server.dll migrate --urls "https://localhost:%PORT%"

endlocal