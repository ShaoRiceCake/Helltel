using UnityEngine;
using System.Collections.Generic; // ����List����������ռ�

public class ToggleScript : MonoBehaviour
{
    // ���ڴ洢����ű����б�
    public CatchControl scriptsToToggle;

    void Update()
    {
        // ����Ƿ��¿ո��
        if (Input.GetKeyDown(KeyCode.Space))
        {
            scriptsToToggle.isCacthing = !scriptsToToggle.isCacthing;
        }
    }
}