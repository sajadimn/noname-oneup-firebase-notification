using System.IO;
using System.Text;
using UnityEngine;

public static class FirebaseNotificationAndroidLibrary
{
    public const string Folder = "Assets/Plugins/Android/FirebaseNotification.androidlib";
    public const string ColorName = "oneup_notification_color";

    private const string MetaPrefix = "com.google.firebase.messaging.";

    public static string GetManifest(FirebaseNotificationConfig config)
    {
        StringBuilder manifest = new StringBuilder();
        manifest.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        manifest.Append("<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.oneup.firebasenotification\">\n");
        manifest.Append("  <application>\n");
        if (!string.IsNullOrEmpty(config.defaultIcon))
            AppendMeta(manifest, "default_notification_icon", $"android:resource=\"@drawable/{config.defaultIcon}\"");
        if (config.useDefaultColor)
            AppendMeta(manifest, "default_notification_color", $"android:resource=\"@color/{ColorName}\"");
        if (!string.IsNullOrEmpty(config.defaultChannelId))
            AppendMeta(manifest, "default_notification_channel_id", $"android:value=\"{config.defaultChannelId}\"");
        manifest.Append("  </application>\n</manifest>\n");
        return manifest.ToString();
    }

    public static string GetColorResource(Color color)
    {
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n" +
               $"  <color name=\"{ColorName}\">#{ColorUtility.ToHtmlStringRGB(color)}</color>\n</resources>\n";
    }

    public static void Write(string folder, FirebaseNotificationConfig config)
    {
        Directory.CreateDirectory(folder);
        WriteIfChanged(Path.Combine(folder, "AndroidManifest.xml"), GetManifest(config));
        WriteIfChanged(Path.Combine(folder, "project.properties"), "target=android-9\nandroid.library=true\n");

        string colorPath = Path.Combine(folder, "res", "values", $"{ColorName}.xml");
        if (config.useDefaultColor)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(colorPath));
            WriteIfChanged(colorPath, GetColorResource(config.defaultColor));
        }
        else if (File.Exists(colorPath))
        {
            File.Delete(colorPath);
        }
    }

    private static void AppendMeta(StringBuilder manifest, string name, string value)
    {
        manifest.Append($"    <meta-data android:name=\"{MetaPrefix}{name}\" {value} />\n");
    }

    private static void WriteIfChanged(string path, string text)
    {
        if (!File.Exists(path) || File.ReadAllText(path) != text)
            File.WriteAllText(path, text);
    }
}
