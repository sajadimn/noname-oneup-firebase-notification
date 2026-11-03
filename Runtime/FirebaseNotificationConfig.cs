using System.Collections.Generic;
using UnityEngine;

public class FirebaseNotificationConfig : ScriptableObject
{
    public const string ResourcePath = "FirebaseNotificationConfig";
    public const string AssetPath = "Assets/Settings/FirebaseNotification/Resources/FirebaseNotificationConfig.asset";

    public string defaultIcon = "";
    public bool useDefaultColor;
    public Color defaultColor = Color.white;
    public string defaultChannelId = "";
    public List<string> topics = new List<string>();
    public bool requestPermissionOnInit;

    public static FirebaseNotificationConfig Load()
    {
        FirebaseNotificationConfig config = Resources.Load<FirebaseNotificationConfig>(ResourcePath);
        return config != null ? config : CreateInstance<FirebaseNotificationConfig>();
    }
}
