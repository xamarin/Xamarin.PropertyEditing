#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://xamjenkinsartifact.azureedge.net/build-package-osx-mono/2019-10/76/77147e75266a61a0c57ec70083802025387654bf/MonoFramework-MDK-6.8.0.73.macos10.xamarin.universal.pkg");
Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode11.3/5e52f30c0545ccf8aee448dffb96ce3e88463987/13/package/notarized/xamarin.mac-6.9.0.13.pkg");
Item (XreItem.Xcode_11_3_0).XcodeSelect();
