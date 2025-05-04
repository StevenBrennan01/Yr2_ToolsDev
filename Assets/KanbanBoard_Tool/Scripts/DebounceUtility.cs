using System;
using UnityEditor;
using UnityEngine;

public class DebounceUtility
{
    private static Action debouncedAction;
    private static float debounceTime;
    private static float lastActionTime;

    public static void Debounce(Action action, float debounceTime)
    {
        debouncedAction = action;
        DebounceUtility.debounceTime = debounceTime;
        lastActionTime = Time.realtimeSinceStartup;

        EditorApplication.update += FixedUpdate;
    }
    
    public static void FixedUpdate()
    {
        if (Time.realtimeSinceStartup - lastActionTime >= debounceTime)
        {
            debouncedAction?.Invoke();
            EditorApplication.update -= FixedUpdate;
        }
    }
}