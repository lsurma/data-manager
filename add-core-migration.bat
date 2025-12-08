@echo off
setlocal enabledelayedexpansion

REM Configuration
SET MIGRATION_NAME=%1
set "MIGRATIONS_PROJECT=DataManager.Application.Core\DataManager.Application.Core.csproj"
set "STARTUP_PROJECT=DataManager.Host.AzFuncAPI\DataManager.Host.AzFuncAPI.csproj"
set "CONTEXT=DataManagerDbContext"
set "MIGRATIONS_DIR=Data\Migrations"

dotnet ef migrations add %MIGRATION_NAME% --project "%MIGRATIONS_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output-dir "%MIGRATIONS_DIR%" --context "%CONTEXT%"