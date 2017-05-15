@echo off
 .\tools\Opencover\OpenCover.Console.exe -register:user -target:".\tools\xunit\xunit.console.x86.exe" -targetargs:".\src\EventSourcing.Tests.Unit\bin\Debug\EventSourcing.Tests.Unit.dll -noshadow" -output:".\build\coverage.xml" -filter:"+[EventSourcing*]* -[EventSourcing]EventSourcing.Properties.* -[EventSourcing.Tests.Unit*]* -[EventSourcing.Samples.Core*]*"
if %ERRORLEVEL% == 0 goto :next

:quit
exit /b %ERRORLEVEL%

:next
@echo.
@echo %date%
@echo %time%
@echo.