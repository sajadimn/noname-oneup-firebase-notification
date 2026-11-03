using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

[InitializeOnLoad]
public static class FirebaseNotificationSdkInstaller
{
    private const string ManifestPath = "Packages/manifest.json";
    private const string LocalPackagesFolder = "LocalPackages";
    private const string FirebaseUrl = "https://dl.google.com/games/registry/unity/";
    private const string UpdateKey = "OneUp.FirebaseNotification.Update";
    private const string AttemptsKey = "OneUp.FirebaseNotification.InstallAttempts";
    private const string PartExtension = ".part";
    private const int MaxAttempts = 12;

    private static bool isRunning;
    private static UnityWebRequest request;
    private static string downloadPath;

    static FirebaseNotificationSdkInstaller()
    {
        if (!Application.isBatchMode)
            EditorApplication.delayCall += RunNextStep;
    }

    [MenuItem("OneUp/FirebaseNotification/Install SDKs")]
    public static void Install()
    {
        SessionState.SetInt(AttemptsKey, 0);
        RunNextStep();
    }

    [MenuItem("OneUp/FirebaseNotification/Update SDKs")]
    public static void UpdateSdks()
    {
        List<string> lines = FirebaseNotificationManifestEditor
            .GetChanges(File.ReadAllText(ManifestPath), Directory.Exists, FirebaseNotificationSdkVersionsConfig.Load())
            .Select(x => $"{x.package}: {x.current} > {x.target}").ToList();

        if (lines.Count == 0)
        {
            EditorUtility.DisplayDialog("Update SDKs", "All SDKs already match FirebaseNotificationSdkVersionsConfig.", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Update SDKs", string.Join("\n", lines), "Update", "Cancel"))
        {
            SessionState.SetBool(UpdateKey, true);
            SessionState.SetInt(AttemptsKey, 0);
            RunNextStep();
        }
    }

    private static void RunNextStep()
    {
        int attempts = SessionState.GetInt(AttemptsKey, 0);
        if (isRunning || attempts >= MaxAttempts)
            return;

        string manifest = File.ReadAllText(ManifestPath);
        bool updating = SessionState.GetBool(UpdateKey, false);
        if (!updating && FirebaseNotificationManifestEditor.HasSdks(manifest, Directory.Exists))
            return;

        FirebaseNotificationSdkVersions versions = FirebaseNotificationSdkVersionsConfig.Load();
        string[] missingFiles = FirebaseNotificationManifestEditor.GetMissingFiles(File.Exists, Directory.Exists, versions);

        isRunning = true;
        SessionState.SetInt(AttemptsKey, attempts + 1);
        Debug.Log("FirebaseNotificationSdkInstaller - Installing the Firebase Messaging SDK.");

        if (missingFiles.Length > 0)
        {
            Download(missingFiles[0], versions);
            return;
        }

        string updated = updating
            ? FirebaseNotificationManifestEditor.ApplyVersions(manifest, Directory.Exists, versions)
            : FirebaseNotificationManifestEditor.AddSdks(manifest, Directory.Exists, versions);
        File.WriteAllText(ManifestPath, updated);
        if (updating)
            RemoveOldFiles(versions);

        SessionState.EraseBool(UpdateKey);
        isRunning = false;
        Client.Resolve();
    }

    private static void RemoveOldFiles(FirebaseNotificationSdkVersions versions)
    {
        foreach (string package in FirebaseNotificationManifestEditor.GetFirebasePackages())
        {
            string current = FirebaseNotificationManifestEditor.GetFileName(package, versions.firebase);
            foreach (string file in Directory.GetFiles(LocalPackagesFolder, $"{package}-*.tgz"))
            {
                if (Path.GetFileName(file) != current)
                    File.Delete(file);
            }
        }
    }

    private static void Download(string fileName, FirebaseNotificationSdkVersions versions)
    {
        string package = fileName.Replace($"-{versions.firebase}.tgz", "");
        Directory.CreateDirectory(LocalPackagesFolder);
        downloadPath = Path.Combine(LocalPackagesFolder, fileName);
        request = UnityWebRequest.Get($"{FirebaseUrl}{package}/{fileName}");
        request.downloadHandler = new DownloadHandlerFile(downloadPath + PartExtension);
        request.SendWebRequest();
        EditorApplication.update += WaitForDownload;
    }

    private static void WaitForDownload()
    {
        if (!request.isDone)
            return;

        EditorApplication.update -= WaitForDownload;
        bool success = request.result == UnityWebRequest.Result.Success;
        string error = request.error;
        request.Dispose();
        request = null;
        isRunning = false;

        if (success)
        {
            File.Move(downloadPath + PartExtension, downloadPath);
            EditorApplication.delayCall += RunNextStep;
            return;
        }

        File.Delete(downloadPath + PartExtension);
        SessionState.SetInt(AttemptsKey, MaxAttempts);
        Debug.LogError($"FirebaseNotificationSdkInstaller - Failed to download {downloadPath}: {error} Run OneUp/FirebaseNotification/Install SDKs to try again.");
    }
}
