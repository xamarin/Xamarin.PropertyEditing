#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

if (IsMac) {
	DotNetCoreSdk ("../global.json")
		.Workload(
			"microsoft.net.sdk.macos",
			"12.0.101-preview.10.249",
			"https://aka.ms/dotnet6/nuget/index.json",
			"https://api.nuget.org/v3/index.json");
}

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2020-02/77/e9d3af508e46454389cb29836d19616eae1615c0/MonoFramework-MDK-6.12.0.74.macos10.xamarin.universal.pkg");
Item ("https://download.visualstudio.microsoft.com/download/pr/951ce94d-144e-4a6e-9c1b-31066ce2044c/6c24acc6dfa374bb5f311c46e0dddc03/xamarin.mac-7.11.2.4.pkg");
Xcode ("12.4.0").XcodeSelect();
