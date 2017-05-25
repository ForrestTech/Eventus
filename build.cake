#tool "nuget:?package=xunit.runner.console"
#addin "MagicChunks"
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
    NuGetRestore("./src/Eventus.sln");
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

Task("Pack")
    .IsDependentOn("Unit-Tests")
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
