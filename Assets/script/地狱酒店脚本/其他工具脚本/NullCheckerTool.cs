using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public static class NullCheckerTool
{
    /// <summary>
    /// ��鴫��Ĳ����Ƿ�Ϊ null�����Ϊ null �򱨴���ʾ��������
    /// </summary>
    /// <param name="objects">��Ҫ���Ķ������顣</param>
    public static void CheckNull(params object[] objects)
    {
        // ��ȡ�����ߵķ�����Ϣ
        MethodBase callerMethod = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
        ParameterInfo[] parameters = callerMethod.GetParameters();

        // �������д���Ķ���
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                // ��ȡ������
                string paramName = parameters[i].Name;

                // ����
                Debug.LogError($"Parameter '{paramName}' is null in method '{callerMethod.Name}'.");
            }
        }
    }
}