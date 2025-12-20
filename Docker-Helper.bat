@echo off
setlocal enabledelayedexpansion
set runComposeDown=true

REM === Show current Docker context ===
for /f "tokens=*" %%i in ('docker context show') do set CURRENT_CONTEXT=%%i
echo.
echo Current Docker context: %CURRENT_CONTEXT%
echo.

REM === Ask if the user wants to change context ===
set /p CHANGE_CONTEXT="Do you want to change the Docker context? (y/N) [N]: "
if /i "%CHANGE_CONTEXT%"=="y" (
    echo.
    echo Available Docker contexts:
    docker context ls
    echo.
    set /p NEW_CONTEXT="Enter the name of the Docker context to use: "
    docker context use %NEW_CONTEXT%
    echo Switched to context: %NEW_CONTEXT%
    echo.
)

REM === Ask if user wants to only run docker-compose down ===
set /p ONLY_DOWN="Do you want to ONLY run 'docker-compose down' and skip starting containers? (y/N) [N]: "
if /i "%ONLY_DOWN%"=="y" (
    echo Running only docker-compose down...
    docker-compose down
    goto :eof
)

REM === Choose docker-compose file ===
set /p USE_OVERRIDE="Use docker-compose.override.yml instead of docker-compose.prod.yml? (Y/n) [Y]: "
if /i "%USE_OVERRIDE%"=="n" (
    set DOCKER_FILE=docker-compose.prod.yml
) else (
    set DOCKER_FILE=docker-compose.override.yml
)

REM === Ask for a service command (init-db / migrate-db) ===
set "SERVICE_CMD="
echo Optional: enter a service command to run (migrate-db), or press Enter to skip:
set /p DB_COMMAND="Command: "
if not "%DB_COMMAND%"=="" (
    set runComposeDown=false
    set SERVICE_CMD=run imsapi %DB_COMMAND%
)

REM === Ask for detached mode ===
if "%runComposeDown%"=="false" (
    set /p DETACHED="Run detached (-d)? (Y/n) [N]: "
) else (
    set /p DETACHED="Run detached (-d)? (Y/n) [Y]: "
)

if /i "%DETACHED%"=="n" (
    set DETACH_FLAG=
) else (
    set DETACH_FLAG=-d
)

REM === Choose whether to rebuild image ===
set /p USE_BUILD="Rebuild image? (Y/n) [Y]: "
if /i "%USE_BUILD%"=="n" (
    set BUILD_FLAG=
) else (
    set BUILD_FLAG=--build
)

REM === Construct and run final docker-compose command ===
echo.
if "%runComposeDown%"=="true" (
    docker-compose down
)

if not "%SERVICE_CMD%"=="" (
    echo Running: docker-compose -f docker-compose.yml -f %DOCKER_FILE% %SERVICE_CMD%
    docker-compose -f docker-compose.yml -f %DOCKER_FILE% %SERVICE_CMD%
) else (
    echo Running: docker-compose -f docker-compose.yml -f %DOCKER_FILE% up %DETACH_FLAG% %BUILD_FLAG%
    docker-compose -f docker-compose.yml -f %DOCKER_FILE% up %DETACH_FLAG% %BUILD_FLAG%
)

endlocal
pause
