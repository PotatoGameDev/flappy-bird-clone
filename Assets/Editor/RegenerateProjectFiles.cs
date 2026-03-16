using UnityEditor;
using System.Reflection;

public static class RegenerateProjectFiles
{
    [MenuItem("Tools/Regenerate Project Files")]
    public static void Regenerate()
    {
        var editorAssembly = typeof(Editor).Assembly;

        var syncVsType = editorAssembly.GetType("UnityEditor.SyncVS");
        var method = syncVsType.GetMethod(
            "SyncSolution",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
        );

        method.Invoke(null, null);

        UnityEngine.Debug.Log("Project files regenerated.");
    }
}
