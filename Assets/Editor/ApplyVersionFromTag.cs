using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class ApplyVersionFromTag
{
    private const string VersionArg = "-setVersion";

    [MenuItem("Tools/Apply Version From Tag")]
    public static void Apply()
    {
        string[] args = Environment.GetCommandLineArgs();
        string version = null;

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == VersionArg)
            {
                version = args[i + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(version))
        {
            Debug.Log("[ApplyVersionFromTag] No version argument found. Skipping.");
            return;
        }

        version = version.TrimStart('v');
        Debug.Log($"[ApplyVersionFromTag] Applying version: {version}");

        PlayerSettings.bundleVersion = version;

        if (int.TryParse(Regex.Replace(version, @"\D", ""), out int versionCode))
        {
            PlayerSettings.Android.bundleVersionCode = versionCode;
            Debug.Log($"[ApplyVersionFromTag] AndroidBundleVersionCode set to: {versionCode}");
        }
        else
        {
            Debug.LogWarning($"[ApplyVersionFromTag] Could not parse version code from: {version}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ApplyVersionFromTag] Done. ProjectSettings saved.");
    }
}
