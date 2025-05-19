using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  兔先生动画回调方法集合
/// </summary>
public class AudioEventBunny : MonoBehaviour
{
    public void PlayAttack_1()
    {
        AudioManager.Instance.Play("兔子攻击1", transform.position);
    }
    public void PlayAttack_2()
    {
        AudioManager.Instance.Play("兔子攻击2", transform.position);
    }
    public void PlayAttack_3()
    {
        AudioManager.Instance.Play("兔子攻击3", transform.position);
    }

    public void PlayBunnyMove()
    {
        AudioManager.Instance.Play("兔子移动", transform.position);
    }


}
