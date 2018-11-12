#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

Item ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/b40230c09d557991b776de918b047442cd41533f/211/package/xamarin.mac-5.0.0.0.pkg");
Item (XreItem.Xcode_10_0_0).XcodeSelect();
