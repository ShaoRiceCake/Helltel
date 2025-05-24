using UnityEngine;

public class ToggleObjectActivation : MonoBehaviour
{
    // 要控制的对象
    public GameObject targetObject;

    void Update()
    {
        // 检测按键M是否被按下
        if (Input.GetKeyDown(KeyCode.M))
        {
            // 切换目标对象的激活状态
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf);
                
                // 可选：在控制台输出当前状态
            }
            else
            {
                Debug.LogWarning("未关联目标对象！");
            }
        }
    }
}