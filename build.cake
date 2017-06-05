#tool "nuget:?package=xunit.runner.console"
#addin "MagicChunks"
#addin "Cake.Powershell"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var packageVersion = Argument("package-version", "0.1.0");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Eventus/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Eventus.sln", new NuGetRestoreSettings{
		Verbosity = NuGetVerbosity.Quiet 
	});
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/Eventus.sln", settings =>
        settings.SetConfiguration(configuration)
		.SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
		.SetPlatformTarget(PlatformTarget.MSIL)
		.SetMSBuildPlatform(MSBuildPlatform.Automatic));
    }
    else
    {
      // Use XBuild
      XBuild("./src/Eventus.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./src/**/bin/" + configuration + "/*.Tests.Unit.dll");
});

Task("Coverage")
	.Description("Create coverage report")
	.IsDependentOn("Unit-Tests")
	.Does(() =>
{
	StartPowershellScript(@"src\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -filter:'+[Eventus]* -[Eventus]Eventus.Logging.* -[Eventus]Eventus.LibLog.*' -target:'src\packages\xunit.runner.console.2.2.0\tools\xunit.console.exe' -targetargs:'src\Eventus.Tests.Unit\bin\" + configuration  + @"\Eventus.Tests.Unit.dll -noshadow' -output:coverage.xml");
});

Task("Upload-coverage")
	.Description("Upload coverage to coverall")
	.IsDependentOn("Coverage")
	.Does(() =>
{
	StartPowershellScript(@"src\packages\coveralls.io.1.3.4\tools\coveralls.net.exe --opencover coverage.xml");
});

Task("Pack")
    .IsDependentOn("Coverage")
    .Does(() =>
{
    NuGetPack("./src/Eventus/Eventus.csproj", new NuGetPackSettings{
		Version = packageVersion,
		Properties = new Dictionary<string,string>{
			{ "Configuration", configuration }
		}
	});
});

Task("Integration-Tests")
    .IsDependentOn("Unit-Tests")
    .Does(() =>
{
	//Run DocumentDb
	TransformConfig(
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		new TransformationCollection {
			{ "configuration/appSettings/add[@key='Provider']/@value","DocumentDb" }
		});

	Information("Running DocumentDb integration tests");
	XUnit2("./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll");

	//Run Sql server tests
	TransformConfig(
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		new TransformationCollection {
			{ "configuration/appSettings/add[@key='Provider']/@value","SqlServer" }
		});

	Information("Running Sql Server integration tests");
	XUnit2("./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll");

	//Run EventStore
	TransformConfig(
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		new TransformationCollection {
			{ "configuration/appSettings/add[@key='Provider']/@value","EventStore" }
		});

	Information("Running EventStore integration tests");
	XUnit2("./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll");
});


Task("CI-Sql-Test")
    .IsDependentOn("Upload-coverage")
    .Does(() =>
{
	//Run DocumentDb
	TransformConfig(
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		"./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll.config",
		new TransformationCollection {
			{ "configuration/appSettings/add[@key='Provider']/@value","SqlServer" },
			{ "configuration/connectionStrings/add[@name='Eventus']/@connectionString","Server=(local)\\SQL2016;Database=master;User ID=sa;Password=Password12!" }
		});

	Information("Running SqlServer integration tests");

	XUnit2("./src/Eventus.Tests.Integration/bin/" + configuration + "/Eventus.Tests.Integration.dll");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
