using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieController : MonoBehaviour
{
    private GameDataModel _data;
    

    // Start is called before the first frame update
    public void Awake()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnIsPlayerDiedChangedEvent += PlayerDie;
        
    }
    public void OnDestroy()
    {
        _data.OnIsPlayerDiedChangedEvent -= PlayerDie;
    }
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void PlayerDie(bool isDie)
    {
        if(isDie)
        {
            //停止玩家控制
            PlayerControlInformationProcess playersControlInformation = FindObjectOfType<PlayerControlInformationProcess>();
            if(playersControlInformation != null)
            playersControlInformation.stopPlayerControl = true;
            //To DO：最好能有个镜头控制，调转到正上方或者其他比较适合观察死亡的位置（不转也行，因为我们本来就是第三人称）

            //To DO：玩家控制的角色倒地

            //To DO：血液生成器

            //To DO：屏幕血液shader

            //To DO：屏幕变黑白

            //To DO：死亡音效
            AudioManager.Instance.Play("气球爆炸");
            //To DO：渐变为黑色
            //让流程管理器返回开始界面

        }

    }
}
