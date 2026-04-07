#if SUPERDEBUG
using UnityEditor;
using UnityEngine;
using System.Diagnostics;


[InitializeOnLoad]
public static class EditorFreezeDetector
{
    static Stopwatch stopwatch = new Stopwatch();
    static long lastTime;

    static EditorFreezeDetector()
    {
        stopwatch.Start();
        lastTime = stopwatch.ElapsedMilliseconds;

        EditorApplication.update += Update;
    }

    static void Update()
    {
        long now = stopwatch.ElapsedMilliseconds;
        long delta = now - lastTime;

        if (delta > 200) // freeze threshold (ms)
        {
            UnityEngine.Debug.Log($"EDITOR FREEZE: {delta} ms");
            LogContext();
        }

        lastTime = now;
    }

    static void LogContext()
    {
        UnityEngine.Debug.Log("Active object: " + Selection.activeObject);
        UnityEngine.Debug.Log("Time: " + System.DateTime.Now);
    }
}

#endif
