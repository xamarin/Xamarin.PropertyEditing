#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2018-08/248/fdb26b0a4454f60d20df1ea6c01fd851ffa4084a/MonoFramework-MDK-5.18.1.3.macos10.xamarin.universal.pkg");
Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-0/50f7527307faf601c7b7754ac77a839fd5d0c820/26/package/xamarin.mac-5.6.0.25.pkg");
Item (XreItem.Xcode_10_0_0).XcodeSelect();
