#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

if (IsMac) {
	DotNetCoreSdk ("../global.json", installDirectory: Env("DOTNET_ROOT"))
		.WithWorkload(
			workload: new DotNetWorkload("microsoft.net.sdk.macos", "12.1.301-preview.13.10"),
			dependencies: new [] {
				new DotNetWorkload("microsoft.net.workload.mono.toolchain", "6.0.2-mauipre.1.22102.15"),
			},
			// nugetConfigFilePath: "../NuGet.Config"); // TODO: Make this work and remove `sources` below
			sources: new [] {
				"https://aka.ms/dotnet6/nuget/index.json",
				"https://api.nuget.org/v3/index.json",
			});

	Xcode ("13.2.1").XcodeSelect ();
}
else if (IsWindows) {
	DotNetCoreSdk ("../global.json");
}
