#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System.Linq;

static readonly bool CI = !string.IsNullOrEmpty (Environment.GetEnvironmentVariable ("TF_BUILD"));

if (IsMac) {
	const string MacOSVersion = "10.12";
	const string Mojave = "10.14";
	if (OSVersion < new Version (MacOSVersion))
		throw new Exception ($"macOS {MacOSVersion} or newer is required");

	if (CI) {
		foreach (var dir in System.IO.Directory.GetDirectories ("/Applications", "Xcode*"))
			Console.WriteLine ("\tFound: {0}", dir);
	}

	if (OSVersion < new Version (Mojave)) {
		Item (XreItem.Xcode_10_1_0).XcodeSelect ();
	}
	else {
		Item (XreItem.Xcode_11_0_0_rc).XcodeSelect ();

		Console.WriteLine ($"{Environment.NewLine} 🚦Disabling 32bit warning for Mojave.{Environment.NewLine}");
	}

	Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2019-06/175/4f5ed502c6e04c61cbbf6ba3b64db8187c4b6156/MonoFramework-MDK-6.4.0.192.macos10.xamarin.universal.pkg");
	Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode11/e7986d2645323b76c90de095c590bb84a1e26bdb/306/package/xamarin.mac-6.0.0.8.pkg");

	var dotnetVersion = "2.2.203";
	DotNetCoreSdk (dotnetVersion);
}
