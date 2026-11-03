using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class FirebaseNotificationAndroidLibraryWriter : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
            WriteLibrary();
    }

    [MenuItem("OneUp/FirebaseNotification/Write Android Library")]
    public static void WriteLibrary()
    {
        if (!FirebaseNotificationManifestEditor.IsMessagingInstalled())
            return;

        FirebaseNotificationAndroidLibrary.Write(FirebaseNotificationAndroidLibrary.Folder, FirebaseNotificationConfig.Load());
        AssetDatabase.Refresh();
    }
}
