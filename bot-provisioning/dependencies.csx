#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

if (IsMac) {
	DotNetCoreSdk ("../global.json", installDirectory: Env("DOTNET_ROOT"))
		.Workload(
			"microsoft.net.sdk.macos",
			"12.1.301-preview.13.4",
			"https://aka.ms/dotnet6/nuget/index.json",
			"https://api.nuget.org/v3/index.json");

	Xcode ("13.2.1").XcodeSelect ();
}
else if (IsWindows) {
	DotNetCoreSdk ("../global.json");
}
