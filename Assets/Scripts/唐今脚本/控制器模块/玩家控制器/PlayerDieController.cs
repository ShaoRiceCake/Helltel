using System.Collections;
using UnityEngine;


public class PlayerDieController : MonoBehaviour
{
    private GameDataModel _data;
    private PlayerControlInformationProcess _playerControl;


    

    
    [Header("Death Settings")]

    [SerializeField] private float _blackScreenDelay = 3f;

    
    [Header("References")]


    
    private Coroutine _deathSequenceCoroutine;

    private void Awake()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnIsPlayerDiedChangedEvent += PlayerDie;
        
        _playerControl = FindObjectOfType<PlayerControlInformationProcess>();

        

    }

    private void OnDestroy()
    {
        _data.OnIsPlayerDiedChangedEvent -= PlayerDie;
        
        if (_deathSequenceCoroutine != null)
        {
            StopCoroutine(_deathSequenceCoroutine);
        }
    }

    void PlayerDie(bool isDie)
    {
        if (isDie && _deathSequenceCoroutine == null)
        {
            _deathSequenceCoroutine = StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        // 1. 停止玩家控制
        if (_playerControl != null)
        {
            _playerControl.stopPlayerControl = true;
        }

        // 2. 播放死亡动画


        // 3. 播放死亡音效
        AudioManager.Instance.Play("气球爆炸");
        
        // 4. 生成血液效果


        // 5. 屏幕血液效果


        // 6. 移动摄像机到死亡视角（可不做）

        


        // 7. 屏幕变黑白


        // 8. 等待片刻
        yield return new WaitForSeconds(_blackScreenDelay);

        // 9. 渐变为黑色 (使用渐晕效果)

        // 10. 返回开始界面
        
        // GameManager.Instance.ReturnToMainMenu();
        
        _deathSequenceCoroutine = null;
    }
}