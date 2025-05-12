using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    void Start()
    {
        // 启用 Profiler
        Profiler.logFile = "profiler.log";
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;
    }
    
    void Update()
    {
        // 记录自定义标签
        Profiler.BeginSample("MyCustomCode");
        // 你的代码...
        Profiler.EndSample();
    }
}