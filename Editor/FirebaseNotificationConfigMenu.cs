using UnityEditor;

public static class FirebaseNotificationConfigMenu
{
    public static readonly string[] Paths =
    {
        FirebaseNotificationConfig.AssetPath,
        FirebaseNotificationSdkVersionsConfig.AssetPath,
    };

    [MenuItem("OneUp/FirebaseNotification/Configs/Notification")]
    private static void OpenConfig()
    {
        Open(FirebaseNotificationConfig.AssetPath);
    }

    [MenuItem("OneUp/FirebaseNotification/Configs/SDK Versions")]
    private static void OpenVersions()
    {
        Open(FirebaseNotificationSdkVersionsConfig.AssetPath);
    }

    private static void Open(string path)
    {
        FirebaseNotificationConfigGenerator.Generate();
        UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
