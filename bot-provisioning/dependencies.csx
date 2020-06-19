#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2020-02/77/e9d3af508e46454389cb29836d19616eae1615c0/MonoFramework-MDK-6.12.0.74.macos10.xamarin.universal.pkg");
Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-6/088c73638ed0f29f310d362789e7d622cd97dc89/28/package/notarized/xamarin.mac-6.18.0.23.pkg");
Item (XreItem.Xcode_11_5_0).XcodeSelect();
