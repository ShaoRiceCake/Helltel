using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public static class NullCheckerTool
{
    /// <summary>
    /// 检查传入的参数是否为 null，如果为 null 则报错并显示变量名。
    /// </summary>
    /// <param name="objects">需要检查的对象数组。</param>
    public static void CheckNull(params object[] objects)
    {
        // 获取调用者的方法信息
        MethodBase callerMethod = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
        ParameterInfo[] parameters = callerMethod.GetParameters();

        // 遍历所有传入的对象
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                // 获取参数名
                string paramName = parameters[i].Name;

                // 报错
                Debug.LogError($"Parameter '{paramName}' is null in method '{callerMethod.Name}'.");
            }
        }
    }
}