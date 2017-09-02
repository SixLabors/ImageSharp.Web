@echo Off

SET versionCommand=
if not "%GitVersion_NuGetVersion%" == "" (
    SET versionCommand=/p:packageversion=%GitVersion_NuGetVersion%
    @echo building with version set to '%GitVersion_NuGetVersion%'
)

dotnet restore %versionCommand%

ECHO Building projects
dotnet build -c Release %versionCommand%

if not "%errorlevel%"=="0" goto failure

if not "%CI%" == "True"  (
    ECHO NOT on CI server running tests
    dotnet test ./tests/ImageSharp.Web.Tests/ImageSharp.Web.Tests.csproj --no-build -c Release
)
if not "%errorlevel%"=="0" goto failure

ECHO Packaging projects
dotnet pack ./src/ImageSharp.Web/ -c Release --output ../../artifacts --no-build  %versionCommand%
if not "%errorlevel%"=="0" goto failure

where docfx 2>&1 >  NUL
if "%errorlevel%"=="0" (
    docfx metadata
)else (
    set errorlevel=0
)
if not "%errorlevel%"=="0" goto failure

:success
ECHO successfully built project
REM exit 0
goto end

:failure
ECHO failed to build.
REM exit -1
goto end

:end