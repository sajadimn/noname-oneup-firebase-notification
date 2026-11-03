using UnityEditor;
using UnityEngine;

public class FirebaseNotificationSdkVersionsConfig : ScriptableObject
{
    public const string AssetPath = "Assets/Settings/FirebaseNotification/FirebaseNotificationSdkVersionsConfig.asset";

    public FirebaseNotificationSdkVersions versions = new FirebaseNotificationSdkVersions();

    public static FirebaseNotificationSdkVersions Load()
    {
        FirebaseNotificationSdkVersionsConfig config = AssetDatabase.LoadAssetAtPath<FirebaseNotificationSdkVersionsConfig>(AssetPath);
        return config != null ? config.versions : new FirebaseNotificationSdkVersions();
    }
}
