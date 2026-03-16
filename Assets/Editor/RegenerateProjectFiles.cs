using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RegenerateProjectFiles
{
    [MenuItem("Tools/Regenerate Project Files")]
    public static void Regenerate()
    {
        var syncVS = System.Type.GetType("UnityEditor.SyncVS,UnityEditor");
        var syncSolution = syncVS?.GetMethod(
            "SyncSolution",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
        );

        if (syncSolution != null)
        {
            syncSolution.Invoke(null, null);
            Debug.Log("Project files regenerated.");
        }
        else
        {
            Debug.LogError("Could not find SyncVS.SyncSolution method.");
        }
    }
}
