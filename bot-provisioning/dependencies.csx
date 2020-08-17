#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2020-02/92/e59c1cd70f4a7171a0ff5e1f9f4937985d6a4d8d/MonoFramework-MDK-6.12.0.89.macos10.xamarin.universal.pkg");
Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-6/088c73638ed0f29f310d362789e7d622cd97dc89/28/package/notarized/xamarin.mac-6.18.0.23.pkg");
Item (XreItem.Xcode_11_6_0).XcodeSelect();
