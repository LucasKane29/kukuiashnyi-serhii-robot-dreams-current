using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LogService : IService
{
    public void Log(string message, [CallerFilePath] string filePath = "")
        => LogInternal(Debug.Log, filePath, message);

    public void LogWarning(string message, [CallerFilePath] string filePath = "")
        => LogInternal(Debug.LogWarning, filePath, message);

    public void LogError(string message, [CallerFilePath] string filePath = "")
        => LogInternal(Debug.LogError, filePath, message);

    public void LogInternal(Action<string> logAction, string filePath, string message)
    {
        var invoker = Path.GetFileNameWithoutExtension(filePath);
        logAction.Invoke($"<b>[{invoker}]</b> {message}");
    }
}
