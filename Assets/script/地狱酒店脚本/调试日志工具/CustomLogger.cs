using System;
using System.IO;
using UnityEngine;

public static class CustomLogger
{
    private static string _logDirectory;
    private static int _maxLogFiles = 10;

    static CustomLogger()
    {
        _logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
        Application.logMessageReceived += HandleLog;
    }

    // 使用UnityEngine.LogType作为参数类型
    private static void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
    {
        switch (type)
        {
            case UnityEngine.LogType.Error:
                WriteLog(type, logString, stackTrace);
                break;
            case UnityEngine.LogType.Warning:
                WriteLog(type, logString, stackTrace);
                break;
            case UnityEngine.LogType.Log:
                WriteLog(type, logString, stackTrace);
                break;
        }
    }

    private static void WriteLog(UnityEngine.LogType type, string logString, string stackTrace)
    {
        string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}.log";
        string filePath = Path.Combine(_logDirectory, fileName);

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {logString}");
            if (type == UnityEngine.LogType.Error)
            {
                writer.WriteLine(stackTrace);
            }
        }
        CleanupOldLogs();
    }    

    // 清理旧日志文件
    private static void CleanupOldLogs()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(_logDirectory);
        FileInfo[] logFiles = directoryInfo.GetFiles("*.log");

        if (logFiles.Length > _maxLogFiles)
        {
            // 按创建时间排序
            Array.Sort(logFiles, (a, b) => a.CreationTime.CompareTo(b.CreationTime));

            // 删除最旧的文件
            for (int i = 0; i < logFiles.Length - _maxLogFiles; i++)
            {
                logFiles[i].Delete();
            }
        }
    }

    // 设置最大日志文件数量
    public static void SetMaxLogFiles(int maxFiles)
    {
        _maxLogFiles = maxFiles;
    }

    // 手动记录日志（供外部调用）
    public static void Log(string message, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                Debug.LogError(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Log:
                Debug.Log(message);
                break;
        }
    }
}