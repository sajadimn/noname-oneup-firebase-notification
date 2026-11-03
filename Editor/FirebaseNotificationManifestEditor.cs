using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class FirebaseNotificationManifestEditor
{
    public const string FirebaseVersion = "13.14.0";
    public const string DependencyManagerVersion = "1.2.187";
    public const string FirebaseFolder = "Assets/Firebase";
    public const string MessagingDll = "Assets/Firebase/Plugins/Firebase.Messaging.dll";

    private const string RegistryName = "OpenUPM";
    private const string RegistryUrl = "https://package.openupm.com";
    private const string DependencyManagerPackage = "com.google.external-dependency-manager";

    private static readonly string[] FirebasePackages =
    {
        "com.google.firebase.app",
        "com.google.firebase.messaging",
    };

    public static bool IsMessagingInstalled()
    {
#if ONEUP_FIREBASE_MESSAGING_PACKAGE
        return true;
#else
        return System.IO.File.Exists(MessagingDll);
#endif
    }

    public static string[] GetFirebasePackages()
    {
        return FirebasePackages;
    }

    public static string GetFileName(string package, string version)
    {
        return $"{package}-{version}.tgz";
    }

    public static string[] GetMissingFiles(Func<string, bool> fileExists, Func<string, bool> folderExists,
        FirebaseNotificationSdkVersions versions)
    {
        if (folderExists(FirebaseFolder))
            return new string[0];

        return FirebasePackages.Select(x => GetFileName(x, versions.firebase))
            .Where(x => !fileExists($"LocalPackages/{x}"))
            .ToArray();
    }

    public static string AddSdks(string manifestJson, Func<string, bool> folderExists, FirebaseNotificationSdkVersions versions)
    {
        JObject manifest = JObject.Parse(manifestJson);
        foreach (KeyValuePair<string, string> package in GetPackages(folderExists, versions))
            AddDependency(manifest, package.Key, package.Value);

        AddRegistry(manifest);
        return manifest.ToString(Formatting.Indented);
    }

    public static bool HasSdks(string manifestJson, Func<string, bool> folderExists)
    {
        JToken dependencies = JObject.Parse(manifestJson)["dependencies"];
        return folderExists(FirebaseFolder) || FirebasePackages.All(x => dependencies?[x] != null);
    }

    public static List<FirebaseNotificationSdkChange> GetChanges(string manifestJson, Func<string, bool> folderExists,
        FirebaseNotificationSdkVersions versions)
    {
        JObject dependencies = (JObject) JObject.Parse(manifestJson)["dependencies"];
        return GetPackages(folderExists, versions)
            .Where(x => dependencies[x.Key] != null && (string) dependencies[x.Key] != x.Value)
            .Select(x => new FirebaseNotificationSdkChange {package = x.Key, current = (string) dependencies[x.Key], target = x.Value})
            .ToList();
    }

    public static string ApplyVersions(string manifestJson, Func<string, bool> folderExists, FirebaseNotificationSdkVersions versions)
    {
        JObject manifest = JObject.Parse(manifestJson);
        foreach (FirebaseNotificationSdkChange change in GetChanges(manifestJson, folderExists, versions))
            manifest["dependencies"][change.package] = change.target;

        return manifest.ToString(Formatting.Indented);
    }

    private static Dictionary<string, string> GetPackages(Func<string, bool> folderExists, FirebaseNotificationSdkVersions versions)
    {
        Dictionary<string, string> packages = new Dictionary<string, string>();
        if (folderExists(FirebaseFolder))
            return packages;

        foreach (string package in FirebasePackages)
            packages[package] = $"file:../LocalPackages/{GetFileName(package, versions.firebase)}";

        packages[DependencyManagerPackage] = versions.dependencyManager;
        return packages;
    }

    private static void AddDependency(JObject manifest, string package, string version)
    {
        if (manifest["dependencies"] is not JObject dependencies)
        {
            dependencies = new JObject();
            manifest["dependencies"] = dependencies;
        }

        if (dependencies[package] == null)
            dependencies[package] = version;
    }

    private static void AddRegistry(JObject manifest)
    {
        if (manifest["dependencies"]?[DependencyManagerPackage] == null)
            return;

        if (manifest["scopedRegistries"] is not JArray registries)
        {
            registries = new JArray();
            manifest["scopedRegistries"] = registries;
        }

        bool hasScope = registries.OfType<JObject>().SelectMany(x => x["scopes"])
            .Any(x => DependencyManagerPackage.StartsWith((string) x));
        if (hasScope)
            return;

        JObject registry = registries.OfType<JObject>().FirstOrDefault(x => (string) x["url"] == RegistryUrl);
        if (registry == null)
        {
            registry = new JObject {["name"] = RegistryName, ["url"] = RegistryUrl, ["scopes"] = new JArray()};
            registries.Add(registry);
        }

        ((JArray) registry["scopes"]).Add(DependencyManagerPackage);
    }
}
