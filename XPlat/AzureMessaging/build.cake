#tool nuget:?package=XamarinComponent&version=1.1.0.32
#addin nuget:?package=Cake.XCode&version=1.0.8
#addin nuget:?package=Cake.Xamarin.Build&version=1.0.16
#addin nuget:?package=Cake.Xamarin&version=1.3.0.3
#addin nuget:?package=Cake.FileHelpers&version=1.0.3.2

// #load "../../../common.cake"

var TARGET = Argument ("t", Argument ("target", "Default"));

var IOS_VERSION = "423aaba626e3eccdc4770bee8861a8ab8518563b";
var IOS_NUGET_VERSION = "1.2.5.2";
var IOS_URL = string.Format ("https://github.com/Azure/azure-notificationhubs/raw/{0}/iOS/bin/WindowsAzureMessaging.framework.zip", IOS_VERSION);

var ANDROID_VERSION = "0.4";
var ANDROID_NUGET_VERSION = "0.4.0";
var ANDROID_URL = string.Format ("https://dl.bintray.com/microsoftazuremobile/SDK/com/microsoft/azure/notification-hubs-android-sdk/{0}/notification-hubs-android-sdk-{0}.aar", ANDROID_VERSION);

var buildSpec = new BuildSpec {

	Libs = new [] {
		new DefaultSolutionBuilder {
			SolutionPath = "./iOS/source/Xamarin.Azure.NotificationHubs.iOS.sln",
			OutputFiles = new [] { 
				new OutputFileCopy { FromFile = "./iOS/source/bin/unified/Release/Xamarin.Azure.NotificationHubs.iOS.dll" },
			}
		},
		new DefaultSolutionBuilder {
			SolutionPath = "./Android/source/Xamarin.Azure.NotificationHubs.Android.sln",
			OutputFiles = new [] { 
				new OutputFileCopy { FromFile = "./Android/source/bin/Release/Xamarin.Azure.NotificationHubs.Android.dll" },
			}
		},
	},

	Samples = new [] {
		new DefaultSolutionBuilder { SolutionPath = "./iOS/samples/NotificationHubsSampleiOS.sln" },
		new DefaultSolutionBuilder { SolutionPath = "./Android/samples/NotificationHubsSampleAndroid.sln" },
	},

	NuGets = new [] {
		new NuGetInfo { NuSpec = "./nuget/Xamarin.Azure.NotificationHubs.iOS.nuspec", Version = IOS_NUGET_VERSION },
		new NuGetInfo { NuSpec = "./nuget/Xamarin.Azure.NotificationHubs.Android.nuspec", Version = ANDROID_NUGET_VERSION },
	},

	Components = new [] {
		new Component { ManifestDirectory = "./component" },
	}
};

Task ("externals-ios")
	.WithCriteria (!FileExists ("./iOS/externals/sdk.zip"))
	.Does (() => 
{
	EnsureDirectoryExists ("./iOS/externals");

	DownloadFile (IOS_URL, "./iOS/externals/sdk.zip");

	Unzip ("./iOS/externals/sdk.zip", "./iOS/externals");
});
Task ("externals-android")
	.WithCriteria (!FileExists ("./externals/Android/notificationhubs.aar"))
	.Does (() => 
{
	EnsureDirectoryExists ("./Android/externals");

	DownloadFile (ANDROID_URL, "./Android/externals/notificationhubs.aar");
});
Task ("externals").IsDependentOn ("externals-ios").IsDependentOn ("externals-android");

Task ("clean").IsDependentOn ("clean-base").Does (() => 
{
	if (DirectoryExists ("./Android/externals"))
		DeleteDirectory ("./Android/externals", true);

	if (DirectoryExists ("./iOS/externals"))
		DeleteDirectory ("./iOS/externals", true);
});


SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget (TARGET);
