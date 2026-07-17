using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CheckMissingGitFiles
{
    [MenuItem("Tools/Git/Report Missing Files")]
    public static void ReportMissingFiles()
    {
        var missingFiles = GetMissingFiles();
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string logPath = Path.Combine(projectRoot, "missing-git-files.log");

        var lines = new List<string>();
        lines.Add($"=== MISSING FILES: {missingFiles.Count} ===");
        foreach (var file in missingFiles)
        {
            lines.Add(file);
        }

        File.WriteAllLines(logPath, lines);
        Debug.Log($"Report saved to: {logPath}");

        EditorUtility.DisplayDialog("Report",
            $"Found {missingFiles.Count} referenced files not tracked by git.\n\nReport saved to:\n{logPath}",
            "OK");
    }

    [MenuItem("Tools/Git/Add Missing Files")]
    public static void AddMissingFiles()
    {
        var missingFiles = GetMissingFiles();

        if (missingFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing to add", "All referenced files are already tracked by git.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Add Missing Files",
            $"Add {missingFiles.Count} files to git?", "Add", "Cancel"))
        {
            return;
        }

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        int success = 0;
        int failed = 0;

        // Find submodule paths
        var submodules = GetSubmodulePaths(projectRoot);

        foreach (var file in missingFiles)
        {
            try
            {
                // Check if file is in a submodule
                string workingDir = projectRoot;
                string fileArg = file;

                foreach (var sub in submodules)
                {
                    if (file.StartsWith(sub))
                    {
                        workingDir = sub;
                        fileArg = file.Substring(sub.Length + 1);
                        break;
                    }
                }

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"add \"{fileArg}\"",
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                    success++;
                else
                    failed++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to add {file}: {e.Message}");
                failed++;
            }
        }

        Debug.Log($"Added {success} files, failed {failed}");
        EditorUtility.DisplayDialog("Done",
            $"Added: {success}\nFailed: {failed}\n\nRun 'git status' to verify.",
            "OK");
    }

    [MenuItem("Tools/Git/Report Missing Meta Files")]
    public static void ReportMissingMetaFiles()
    {
        var missingMetas = GetMissingMetaFiles();
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string logPath = Path.Combine(projectRoot, "missing-meta-files.log");

        var lines = new List<string>();
        lines.Add($"=== MISSING META FILES: {missingMetas.Count} ===");
        foreach (var file in missingMetas)
        {
            lines.Add(file);
        }

        File.WriteAllLines(logPath, lines);
        Debug.Log($"Report saved to: {logPath}");

        EditorUtility.DisplayDialog("Report",
            $"Found {missingMetas.Count} files without .meta files.\n\nReport saved to:\n{logPath}",
            "OK");
    }

    [MenuItem("Tools/Git/Add Missing Meta Files")]
    public static void AddMissingMetaFiles()
    {
        var missingMetas = GetMissingMetaFiles();

        if (missingMetas.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing to add", "All files have .meta files.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Add Missing Meta Files",
            $"Add {missingMetas.Count} .meta files to git?", "Add", "Cancel"))
        {
            return;
        }

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        int success = 0;
        int failed = 0;
        var submodules = GetSubmodulePaths(projectRoot);

        foreach (var file in missingMetas)
        {
            try
            {
                string metaFile = file + ".meta";
                string workingDir = projectRoot;
                string fileArg = metaFile;

                foreach (var sub in submodules)
                {
                    if (metaFile.StartsWith(sub))
                    {
                        workingDir = sub;
                        fileArg = metaFile.Substring(sub.Length + 1);
                        break;
                    }
                }

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"add \"{fileArg}\"",
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                    success++;
                else
                    failed++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to add {file}.meta: {e.Message}");
                failed++;
            }
        }

        Debug.Log($"Added {success} meta files, failed {failed}");
        EditorUtility.DisplayDialog("Done",
            $"Added: {success}\nFailed: {failed}\n\nRun 'git status' to verify.",
            "OK");
    }

    private static List<string> GetMissingMetaFiles()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        var gitFiles = new HashSet<string>(GetGitTrackedFiles(projectRoot));

        // Also get files from submodules
        var submodules = GetSubmodulePaths(projectRoot);
        foreach (var sub in submodules)
        {
            var subFiles = GetGitTrackedFiles(sub);
            string subName = Path.GetRelativePath(projectRoot, sub).Replace("\\", "/");
            foreach (var f in subFiles)
            {
                gitFiles.Add(subName + "/" + f);
            }
        }

        var trackedAssets = gitFiles
            .Where(f => f.StartsWith("Assets/") && !f.EndsWith(".meta") 
                && !f.Contains("/Library/") && !f.Contains("/.git/"))
            .ToList();

        var missingMetas = new List<string>();
        foreach (var asset in trackedAssets)
        {
            string metaPath = asset + ".meta";
            if (!gitFiles.Contains(metaPath))
            {
                missingMetas.Add(Path.Combine(projectRoot, asset));
            }
        }

        return missingMetas.Distinct().OrderBy(f => f).ToList();
    }

    private static List<string> GetMissingFiles()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);

        var gitFiles = new HashSet<string>(GetGitTrackedFiles(projectRoot));

        var guidToPath = new Dictionary<string, string>();
        var metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Library") && !f.Contains("PackageCache") && !f.Contains(".git"));

        foreach (var metaFile in metaFiles)
        {
            var metaContent = File.ReadAllText(metaFile);
            var guidMatch = Regex.Match(metaContent, @"guid:\s*([a-f0-9]{32})");
            if (guidMatch.Success)
            {
                var relativePath = "Assets" + metaFile.Substring(Application.dataPath.Length)
                    .Replace("\\", "/").Replace(".meta", "");
                guidToPath[guidMatch.Groups[1].Value] = relativePath;
            }
        }

        var sceneFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        var prefabFiles = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        var scriptableObjects = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories)
            .Where(f => !f.Contains("ProjectSettings") && !f.Contains("PackageCache") && !f.Contains("Library"))
            .ToArray();

        var referencedGUIDs = new HashSet<string>();

        foreach (var file in sceneFiles.Concat(prefabFiles).Concat(scriptableObjects))
        {
            var content = File.ReadAllText(file);
            var matches = Regex.Matches(content, @"guid:\s*([a-f0-9]{32})");
            foreach (Match match in matches)
            {
                referencedGUIDs.Add(match.Groups[1].Value);
            }
        }

        var missingFiles = new List<string>();
        foreach (var guid in referencedGUIDs)
        {
            if (guidToPath.TryGetValue(guid, out var filePath))
            {
                if (!gitFiles.Contains(filePath))
                {
                    var absolutePath = Path.Combine(projectRoot, filePath);
                    missingFiles.Add(absolutePath);
                }
            }
        }

        return missingFiles.Distinct().OrderBy(f => f).ToList();
    }

    private static List<string> GetGitTrackedFiles(string rootPath)
    {
        var result = new List<string>();
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "ls-files",
                    WorkingDirectory = rootPath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            result = output.Split('\n')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to run git: {e.Message}");
        }
        return result;
    }

    private static List<string> GetSubmodulePaths(string rootPath)
    {
        var paths = new List<string>();
        var gitmodulesPath = Path.Combine(rootPath, ".gitmodules");

        if (!File.Exists(gitmodulesPath))
            return paths;

        var content = File.ReadAllText(gitmodulesPath);
        var matches = Regex.Matches(content, @"path\s*=\s*(.+)");
        foreach (Match match in matches)
        {
            var path = Path.Combine(rootPath, match.Groups[1].Value.Trim());
            if (Directory.Exists(path))
                paths.Add(path);
        }
        return paths;
    }
}
