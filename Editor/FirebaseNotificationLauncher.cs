using System.Text.RegularExpressions;

public static class FirebaseNotificationLauncher
{
    public const string MessagingActivity = "com.google.firebase.MessagingUnityPlayerActivity";

    private const string LauncherCategory = "android.intent.category.LAUNCHER";

    private static readonly Regex UnityActivity = new Regex(
        "<activity\\b[^>]*android:name=\"(com\\.unity3d\\.player\\.UnityPlayer(?:Game)?Activity)\"[^>]*?(?:/>|>.*?</activity>)",
        RegexOptions.Singleline);

    public static string Apply(string manifest, bool messagingInstalled)
    {
        if (!messagingInstalled || string.IsNullOrEmpty(manifest) || manifest.Contains($"\"{MessagingActivity}\""))
            return manifest;

        foreach (Match match in UnityActivity.Matches(manifest))
        {
            if (!match.Value.Contains(LauncherCategory))
                continue;

            string renamed = match.Value.Replace($"android:name=\"{match.Groups[1].Value}\"", $"android:name=\"{MessagingActivity}\"");
            return manifest.Substring(0, match.Index) + renamed + manifest.Substring(match.Index + match.Length);
        }

        return manifest;
    }
}
