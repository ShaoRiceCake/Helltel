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

    [Header("内容区域")]
    public TextMeshProUGUI contentText;

    [Header("功能标签")]
    public Button[] tabButtons; // 3个按钮：[登记簿, 观察日志, 应对方法]
    public Color normalColor = Color.gray;
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
            // 直接设置不播放动画
            monsterNameText.text = monsters[currentIndex].monsterName;
            monsterImage.sprite = monsters[currentIndex].monsterImage;
            UpdateContent();
            UpdateButtonStates();
            return;
        }
        
        if(isAnimating) return;
        isAnimating = true;
        
        // 怪物名称直接切换（无动画）
        monsterNameText.text = monsters[currentIndex].monsterName;
        
        // 仅保留图片淡入淡出动画
        monsterImage.DOFade(0, imageFadeDuration/2).OnComplete(() => 
        {
            monsterImage.sprite = monsters[currentIndex].monsterImage;
            monsterImage.DOFade(1, imageFadeDuration/2).OnComplete(() => 
            {
                isAnimating = false;
            });
        });
        
        UpdateContent();
        UpdateButtonStates();
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
        if(isAnimating) return;
        currentIndex = (currentIndex + 1) % monsters.Length;
        UpdateDisplay();
    }

    public void PreviousMonster()
    {
        if(isAnimating) return;
        currentIndex = (currentIndex - 1 + monsters.Length) % monsters.Length;
        UpdateDisplay();
    }

    public void SwitchTab(int tabIndex)
    {
        if(isAnimating || currentTabIndex == tabIndex) return;
        currentTabIndex = tabIndex;
        
        // 按钮点击动画
        tabButtons[tabIndex].transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
        
        UpdateContent();
        UpdateButtonStates();
    }
}