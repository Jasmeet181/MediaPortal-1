REM detect if BUILD_TYPE should be release or debug
if not %1!==Debug! goto RELEASE
:DEBUG
set BUILD_TYPE=Debug
goto START
:RELEASE
set BUILD_TYPE=Release
goto START


:START
REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

REM Define MSbuild path
if not defined MSBUILD_PATH set MSBUILD_PATH=%progpath%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe
if not exist "%MSBUILD_PATH%" set MSBUILD_PATH=%progpath%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe
if not exist "%MSBUILD_PATH%" set MSBUILD_PATH=%progpath%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\bin\MSBuild.exe
if not exist "%MSBUILD_PATH%" set MSBUILD_PATH=%progpath%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\bin\MSBuild.exe

REM set other MP related paths
set GIT_ROOT=..
set DeployVersionGIT="%GIT_ROOT%\Tools\Script & Batch tools\DeployVersionGIT\DeployVersionGIT\bin\Release\DeployVersionGIT.exe"

set CommonMPTV="%GIT_ROOT%\Common-MP-TVE3"
set DirectShowFilters="%GIT_ROOT%\DirectShowFilters"
set MediaPortal="%GIT_ROOT%\mediaportal"
set TVLibrary="%GIT_ROOT%\TvEngine3\TVLibrary"
set LibblurayJAR="%GIT_ROOT%\libbluray\src\libbluray\bdj\build.xml"
set NugetPackages=%GIT_ROOT%\Packages

REM set log file
set log=%project%_%BUILD_TYPE%.log

REM init log file, write dev env...
echo.
echo. > %log%
echo -= MSBUILD PATH =- >> %log%
echo -= %MSBUILD_PATH% =- >> %log%
echo -= %project% =-
echo -= %project% =- >> %log%
echo -= build mode: %BUILD_TYPE% =-
echo -= build mode: %BUILD_TYPE% =- >> %log%
echo.
echo. >> %log%

echo. >> %log%
echo Using following environment variables: >> %log%
echo DXSDK_DIR = %DXSDK_DIR% >> %log%
echo. >> %log%

REM copy BuildReport resources
xcopy /I /Y .\BuildReport\_BuildReport_Files .\_BuildReport_Files

REM Download NuGet packages
@"%MSBUILD_PATH%" RestorePackages.targets