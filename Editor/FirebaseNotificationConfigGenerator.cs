using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FirebaseNotificationConfigGenerator
{
    static FirebaseNotificationConfigGenerator()
    {
        EditorApplication.delayCall += Generate;
    }

    [MenuItem("OneUp/FirebaseNotification/Create Missing Configs")]
    public static void Generate()
    {
        CreateFolder("Assets", "Settings");
        CreateFolder("Assets/Settings", "FirebaseNotification");
        CreateFolder("Assets/Settings/FirebaseNotification", "Resources");
        Create<FirebaseNotificationConfig>(FirebaseNotificationConfig.AssetPath);
        Create<FirebaseNotificationSdkVersionsConfig>(FirebaseNotificationSdkVersionsConfig.AssetPath);
        FirebaseNotificationAndroidLibraryWriter.WriteLibrary();
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static void Create<T>(string path) where T : ScriptableObject
    {
        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets)))
            return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
        Debug.Log($"FirebaseNotificationConfigGenerator - Created {path}");
    }
}
