using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuestBookPanel : MonoBehaviour
{
    private GlobalUIController globalUIController;

    [Header("设置界面按钮")]
    public Button btn_BackOfGuestBookPanel; //返回按钮
    private void Start()
    {
        globalUIController = GlobalUIController.Instance.GetComponent<GlobalUIController>();

        // 绑定按钮事件
        btn_BackOfGuestBookPanel.onClick.AddListener(globalUIController.CloseGuestBook);
        
    }
}
