using System;
using UnityEditor;
using UnityEngine;

public class DebounceUtility
{
    private static Action _debouncedAction;
    private static float _debounceTime;
    private static float _lastActionTime;

    // Delay/Debounce the action(saving and refreshing) for the set time
    public static void Debounce(Action action, float debounceTime)
    {
        _debouncedAction = action;
        _debounceTime = debounceTime;
        _lastActionTime = Time.realtimeSinceStartup;

        EditorApplication.update += Update;
    }
    
    public static void Update()
    {
        if (Time.realtimeSinceStartup - _lastActionTime >= _debounceTime)
        {
            _debouncedAction?.Invoke();
            EditorApplication.update -= Update;
        }
    }
}