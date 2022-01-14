#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

if (IsMac) {
	DotNetCoreSdk ("../global.json")
		.Workload(
			"microsoft.net.sdk.macos",
			"12.0.101-preview.10.249",
			"https://aka.ms/dotnet6/nuget/index.json",
			"https://api.nuget.org/v3/index.json");

	Xcode ("13.2.1").XcodeSelect ();
}
else if (IsWindows) {
	DotNetCoreSdk ("../global.json");
}
