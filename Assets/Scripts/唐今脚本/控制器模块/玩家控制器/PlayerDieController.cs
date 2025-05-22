using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerDieController : MonoBehaviour
{
    private GameDataModel _data;
    private PlayerControlInformationProcess _playerControl;
    
    [Header("Death Settings")]
    private float _blackScreenDelay = 8f;
    
    [Header("References")]
    [SerializeField]  private GameObject upCube;
    private Coroutine _deathSequenceCoroutine;
    

    private void Awake()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnIsPlayerDiedChangedEvent += PlayerDie;
        
        _playerControl = GetComponent<PlayerControlInformationProcess>();
    }

    private void OnDestroy()
    {
        _data.OnIsPlayerDiedChangedEvent -= PlayerDie;
        
        if (_deathSequenceCoroutine != null)
        {
            StopCoroutine(_deathSequenceCoroutine);
        }
    }

    private void PlayerDie(bool isDie)
    {
        if (isDie && _deathSequenceCoroutine == null)
        {
            _deathSequenceCoroutine = StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        // 停止玩家控制
        if (_playerControl)
        {
            _playerControl.stopPlayerControl = true;
        }
        
        // 生成血液效果
        var emitterCount = 1000;
        var emissionSpeed = 2;
        var randomness = 0.2f;
        var rotation = Quaternion.identity;
        EventBus<BloodSprayEvent>.Publish(
            new BloodSprayEvent(
                upCube.transform.position,
                rotation,
                emitterCount,
                emissionSpeed,
                randomness
            )
        );
        
        BloodEffectController.ActivateBloodEffect();

        // 播放死亡动画
        upCube.SetActive(false);
        
        // 播放死亡音效
        AudioManager.Instance.Play("死亡");

        yield return new WaitForSeconds(_blackScreenDelay);
        // // 返回开始界面
        // if(GameController.Instance != null)
        // {
        //     GameController.Instance.lSS_Manager.LoadScene();
        // }
        SceneManager.LoadSceneAsync($"单机正式菜单");        
        _deathSequenceCoroutine = null;
    }
}