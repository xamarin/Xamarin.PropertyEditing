#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2020-02/144/b4a385816ed4f1398d0184c38f19f560e868fd80/MonoFramework-MDK-6.12.0.137.macos10.xamarin.universal.pkg");
Item ("https://bosstoragemirror.blob.core.windows.net/wrench/main/c8b6bc6c85a0067387ee298ef5e7d55992be5f0a/4590608/package/notarized/xamarin.mac-7.11.0.247.pkg");

Xcode ("12.4.0").XcodeSelect();
