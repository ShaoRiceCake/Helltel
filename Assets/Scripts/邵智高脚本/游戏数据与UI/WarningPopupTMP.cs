using UnityEngine;
using TMPro;
using System.Collections;

public class WarningPopupTMP : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 2.0f;
    [SerializeField] private float displayDuration = 1.5f;

    [SerializeField] private TMP_Text warningText; // 改为序列化字段便于调试

    private void Awake()
    {
        // 如果没手动赋值，尝试自动查找
        if (warningText == null)
        {
            warningText = GetComponentInChildren<TMP_Text>(true);
            
            // 如果还是没找到，报错提示
            if (warningText == null)
            {
                Debug.LogError("找不到 TMP_Text 组件！请确保该脚本挂载在包含 TMP_Text 的对象上", this);
                return;
            }
        }
        
        gameObject.SetActive(false);
    }

    public void ShowWarning(string message)
    {
        if (warningText == null)
        {
            Debug.LogError("TMP_Text 组件未初始化！", this);
            return;
        }

        StopAllCoroutines();
        warningText.text = message;
        gameObject.SetActive(true);

        Color textColor = warningText.color;
        textColor.a = 1f;
        warningText.color = textColor;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(displayDuration);

        float elapsedTime = 0f;
        Color textColor = warningText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textColor.a = alpha;
            warningText.color = textColor;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}