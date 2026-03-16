// Assets/Editor/GitContextMenu.cs
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

public class GitContextMenu
{
    [MenuItem("Assets/Git/Add this file", false, 1000)]
    static void GitAdd()
    {
        var obj = Selection.activeObject;
        string assetPath = AssetDatabase.GetAssetPath(obj);
        string fullPath = Path.GetFullPath(assetPath);

        string repoRoot = GetSubmoduleRoot(fullPath);
        RunGitIn(repoRoot, $"add \"{fullPath}\"", out string output, out string error);

        if (string.IsNullOrEmpty(error))
            UnityEngine.Debug.Log($"[Git] Added: {assetPath}");
        else
            UnityEngine.Debug.LogError($"[Git] Error: {error}");
    }

    [MenuItem("Assets/Git/Add + Commit this file", false, 1001)]
    static void GitAddAndCommit()
    {
        var obj = Selection.activeObject;
        string assetPath = AssetDatabase.GetAssetPath(obj);
        string fullPath = Path.GetFullPath(assetPath);
        string metaPath = fullPath + ".meta";
        string fileName = Path.GetFileName(assetPath);

        string repoRoot = GetSubmoduleRoot(fullPath);

        RunGitIn(repoRoot, $"add \"{fullPath}\"", out _, out string err1);
        if (File.Exists(metaPath))
            RunGitIn(repoRoot, $"add \"{metaPath}\"", out _, out _);

        if (!string.IsNullOrEmpty(err1))
        {
            UnityEngine.Debug.LogError($"[Git] Add error: {err1}");
            return;
        }

        RunGitIn(repoRoot, $"commit -m \"Add asset: {fileName}\"", out string output, out string err2);

        if (string.IsNullOrEmpty(err2))
            UnityEngine.Debug.Log($"[Git] Committed: {fileName}\n{output}");
        else
            UnityEngine.Debug.LogError($"[Git] Commit error: {err2}");
    }

    // Pyta gita gdzie jest root repo dla danego pliku (działa też dla submodułów)
    static string GetSubmoduleRoot(string filePath)
    {
        string dir = Path.GetDirectoryName(filePath);
        RunGitIn(dir, "rev-parse --show-toplevel", out string root, out _);
        return root.Trim();
    }

    static void RunGitIn(string workingDir, string args, out string output, out string error)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        output = proc.StandardOutput.ReadToEnd();
        error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
    }
}
