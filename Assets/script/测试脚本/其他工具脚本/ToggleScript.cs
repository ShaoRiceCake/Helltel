using UnityEngine;
using System.Collections.Generic; // 引入List所需的命名空间

public class ToggleScript : MonoBehaviour
{
    // 用于存储多个脚本的列表
    public CatchControl scriptsToToggle;

    void Update()
    {
        // 检测是否按下空格键
        if (Input.GetKeyDown(KeyCode.Space))
        {
            scriptsToToggle.isCacthing = !scriptsToToggle.isCacthing;
        }
    }
}