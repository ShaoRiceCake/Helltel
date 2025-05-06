using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInitailizeEvent{
    
    public Transform identifiyer;
    
    public PlayerInitailizeEvent(Transform identifiyer)
    {   
        Debug.Log("Event 注册玩家标识符" + identifiyer.name);
        this.identifiyer = identifiyer;
    }
}
public class PlayerDieEvent{

}
public class PlayeRegister : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform identifiyer;
    void Awake()
    {
        if(identifiyer == null)
        {
            identifiyer = this.transform.Find("PlayerTarget");
        }
        EventBus<PlayerInitailizeEvent>.Publish(new PlayerInitailizeEvent(identifiyer));
    }

    void OnDestroy()
    {
        EventBus<PlayerDieEvent>.Publish(new PlayerDieEvent());
    }
}
