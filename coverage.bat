@echo off
 .\tools\Opencover\OpenCover.Console.exe -register:user -target:".\tools\xunit\xunit.console.x86.exe" -targetargs:".\src\Eventus.Tests.Unit\bin\Debug\Eventus.Tests.Unit.dll -noshadow" -output:".\build\coverage.xml" -filter:"+[Eventus*]* -[EEventus]Eventus.Properties.* -[Eventus.Tests.Unit*]* -[Eventus.Samples.Core*]*"
if %ERRORLEVEL% == 0 goto :next

:quit
exit /b %ERRORLEVEL%

:next
@echo.
@echo %date%
@echo %time%
@echo.