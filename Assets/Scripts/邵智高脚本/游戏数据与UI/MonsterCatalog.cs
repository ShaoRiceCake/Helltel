using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MonsterCatalog : MonoBehaviour
{
    [Header("怪物数据")]
    public MonsterData[] monsters;

    [Header("导航控制")]
    public Button leftArrow;
    public Button rightArrow;

    [Header("名称显示")]
    public TextMeshProUGUI monsterNameText; // 只保留一个名称显示

    [Header("怪物图像")]
    public Image monsterImage;
    public float imageFadeDuration = 0.4f;
    public bool preserveAspect = true; // 新增：保持图片比例

    [Header("内容区域")]
    public TextMeshProUGUI contentText;

    [Header("功能标签")]
    public Button[] tabButtons; // 3个按钮：[登记簿, 观察日志, 应对方法]
    private Color normalColor = new Color(0.25f,0.25f,0.25f);
    public Color highlightedColor = Color.white;

    private int currentIndex = 0;
    private int currentTabIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        // 初始化显示
        UpdateDisplay(true);
        
        // 按钮监听
        leftArrow.onClick.AddListener(PreviousMonster);
        rightArrow.onClick.AddListener(NextMonster);
        
        // 3个标签按钮监听
        for(int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
    }

    void UpdateDisplay(bool immediate = false)
    {
        if(immediate)
        {
            monsterNameText.text = monsters[currentIndex].monsterName;
            UpdateMonsterImage(monsters[currentIndex].monsterImage);
            UpdateContent();
            UpdateButtonStates();
            return;
        }
        
        if(isAnimating) return;
        isAnimating = true;
        
        monsterNameText.text = monsters[currentIndex].monsterName;
        
        // 图片切换动画
        monsterImage.DOFade(0, imageFadeDuration/2).OnComplete(() => 
        {
            UpdateMonsterImage(monsters[currentIndex].monsterImage);
            monsterImage.DOFade(1, imageFadeDuration/2).OnComplete(() => 
            {
                isAnimating = false;
            });
        });
        
        UpdateContent();
        UpdateButtonStates();
    }


    [Header("图片显示设置")]
    [Tooltip("图片最大显示宽度")] public float maxWidth = 400f;
    [Tooltip("图片最大显示高度")] public float maxHeight = 400f;
    [Tooltip("图片最小显示宽度")] public float minWidth = 250f;
    [Tooltip("图片最小显示高度")] public float minHeight = 250f;

    void UpdateMonsterImage(Sprite newSprite)
    {
        if (newSprite == null || monsterImage == null) return;

        monsterImage.sprite = newSprite;
        monsterImage.preserveAspect = true; // 保持比例

        RectTransform rt = monsterImage.GetComponent<RectTransform>();
        Texture2D texture = newSprite.texture;
        
        // 获取图片原始尺寸
        float originalWidth = texture.width;
        float originalHeight = texture.height;
        float ratio = originalWidth / originalHeight;

        // 计算理想尺寸（保持比例）
        float targetWidth, targetHeight;
        
        if (originalWidth > originalHeight)
        {
            // 宽图：以宽度为基准
            targetWidth = Mathf.Clamp(originalWidth, minWidth, maxWidth);
            targetHeight = targetWidth / ratio;
            
            // 如果高度仍然超出限制，则以高度为基准
            if (targetHeight > maxHeight)
            {
                targetHeight = maxHeight;
                targetWidth = targetHeight * ratio;
            }
        }
        else
        {
            // 高图：以高度为基准
            targetHeight = Mathf.Clamp(originalHeight, minHeight, maxHeight);
            targetWidth = targetHeight * ratio;
            
            // 如果宽度仍然超出限制，则以宽度为基准
            if (targetWidth > maxWidth)
            {
                targetWidth = maxWidth;
                targetHeight = targetWidth / ratio;
            }
        }

        // 最终确保在最小/最大范围内
        targetWidth = Mathf.Clamp(targetWidth, minWidth, maxWidth);
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

        // 应用尺寸
        rt.sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    void UpdateContent()
    {
        string newText = "";
        switch(currentTabIndex)
        {
            case 0: newText = monsters[currentIndex].registryText; break;   // 登记簿
            case 1: newText = monsters[currentIndex].observationText; break; // 观察日志
            case 2: newText = monsters[currentIndex].solutionText; break;    // 应对方法
        }
        
        // 内容文本淡入淡出动画
        contentText.DOFade(0, 0.2f).OnComplete(() => 
        {
            contentText.text = newText;
            contentText.DOFade(1, 0.2f);
        });
    }

    void UpdateButtonStates()
    {
        for(int i = 0; i < tabButtons.Length; i++)
        {
            Image buttonImage = tabButtons[i].GetComponent<Image>();
            if(buttonImage != null)
            {
                buttonImage.color = (i == currentTabIndex) ? highlightedColor : normalColor;
            }
        }
    }

    public void NextMonster()
    {
        if(isAnimating) {
        Debug.Log("正在动画中，忽略点击");
        return;
        }
        Debug.Log($"切换到下一个怪物，当前索引: {currentIndex}");
       
        AudioManager.Instance.Play("翻页");
        currentIndex = (currentIndex + 1) % monsters.Length;
        UpdateDisplay();
    }

    public void PreviousMonster()
    {
        if(isAnimating) return;
        AudioManager.Instance.Play("翻页");
        currentIndex = (currentIndex - 1 + monsters.Length) % monsters.Length;
        UpdateDisplay();
    }

    public void SwitchTab(int tabIndex)
    {
        if(isAnimating || currentTabIndex == tabIndex) return;
        currentTabIndex = tabIndex;
        AudioManager.Instance.Play("翻页");
        
        // 按钮点击动画
        tabButtons[tabIndex].transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
        
        UpdateContent();
        UpdateButtonStates();
    }
}