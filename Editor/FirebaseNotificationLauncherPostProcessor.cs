using System.IO;
using UnityEditor.Android;

public class FirebaseNotificationLauncherPostProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0;

    public static string GetManifestPath(string unityLibraryPath)
    {
        return Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = GetManifestPath(path);
        if (!File.Exists(manifestPath))
            return;

        string manifest = File.ReadAllText(manifestPath);
        string updated = FirebaseNotificationLauncher.Apply(manifest, FirebaseNotificationManifestEditor.IsMessagingInstalled());
        if (updated != manifest)
            File.WriteAllText(manifestPath, updated);
    }
}
