#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2019-08/82/70d690305348cb30cf620db0679ba1173dc7adb0/MonoFramework-MDK-6.6.0.82.macos10.xamarin.universal.pkg");
Item ("https://download.visualstudio.microsoft.com/download/pr/336d5c3a-3c2f-4b7d-bfad-747e5e685f58/f812b77be77cb9acc728e97e49dce1dd/xamarin.mac-6.2.0.47.pkg");
Item (XreItem.Xcode_11_1_0).XcodeSelect();
