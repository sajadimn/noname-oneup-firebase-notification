using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

[InitializeOnLoad]
public static class FirebaseNotificationSdkDefine
{
    public const string Define = "ONEUP_FIREBASE_MESSAGING";

    static FirebaseNotificationSdkDefine()
    {
        Apply(NamedBuildTarget.Android);
        Apply(NamedBuildTarget.iOS);
    }

    private static void Apply(NamedBuildTarget target)
    {
        string[] defines = PlayerSettings.GetScriptingDefineSymbols(target)
            .Split(';')
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        bool installed = File.Exists(FirebaseNotificationManifestEditor.MessagingDll);
        if (installed == defines.Contains(Define))
            return;

        string[] updated = installed ? defines.Append(Define).ToArray() : defines.Where(x => x != Define).ToArray();
        PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", updated));
    }
}
