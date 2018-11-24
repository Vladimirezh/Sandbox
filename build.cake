var target = Argument<string>("target", "CreateNugetPackage");
var configuration = Argument<string>("configuration", "Release");

#Tool "nuget:?package=xunit.runner.console"
#addin "Cake.FileHelpers"


var sandboxBuild = Directory("./src/Sandbox/bin") + Directory(configuration);
var sandboxClientx86Build = Directory("./src/SandboxClient/bin/x86/Release");
var sandboxClientx64Build = Directory("./src/SandboxClientx64/bin/x64/Release");
var sandboxClientAnyCPUBuild = Directory("./src/SandboxClientAnyCPU/bin/Release");
var nuspecDestFile = Directory("./Nuget") + File("Sandbox.nuspec");
var buildVersion = System.Environment.GetEnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "1.0.0.0";

Task("Clean")
        .Does(()=>{
            CleanDirectory(sandboxBuild);
            CleanDirectory(sandboxClientx86Build);
            CleanDirectory(sandboxClientx64Build);
            CleanDirectory(sandboxClientAnyCPUBuild);
        });


Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
     NuGetRestore("./src/Sandbox.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("SetVersion")
    .Does(() =>
{
     Information("Start Build");

     MSBuild("./src/Sandbox.sln", new MSBuildSettings {
                                ToolVersion = MSBuildToolVersion.VS2017,
                                PlatformTarget = PlatformTarget.MSIL,
                                Configuration = configuration }
            );

});

Task("Test")
    .IsDependentOn("Build")
    .Does(()=>
{
         Information("Start Running Tests");

         XUnit2("./src/SandboxTest/bin/Release/SandboxTest.dll" );
});

Task("CopyOutputToNugetFolder")
    .IsDependentOn("Test")
    .Does(() =>
{
    var dllFile = File("Sandbox.dll");
    CreateDirectory("./Nuget/lib/net452/");

    CopyFile(sandboxBuild + dllFile, Directory("./Nuget/lib/net452/") + dllFile);
    CopyFile(File("Sandbox.nuspec"), nuspecDestFile);
});

Task("SetVersion")
   .Does(() => {
       ReplaceRegexInFiles("./src/*/*/AssemblyInfo.cs", 
                           "(?<=AssemblyVersion\\( \")(.*?)(?=\" \\))", 
                           buildVersion);
       ReplaceRegexInFiles("./src/*/*/AssemblyInfo.cs", 
                           "(?<=AssemblyFileVersion\\( \")(.*?)(?=\" \\))", 
                           buildVersion);
   });
    
Task("CreateNugetPackage")
    .IsDependentOn("CopyOutputToNugetFolder")
    .Does(()=>
{
    
    Information("Building Sandbox.{0}.nupkg", buildVersion);
    var nuGetPackSettings = new NuGetPackSettings {
        Version  = buildVersion,
        OutputDirectory = "./Nuget"
    };
    NuGetPack(nuspecDestFile, nuGetPackSettings);

});


RunTarget(target);