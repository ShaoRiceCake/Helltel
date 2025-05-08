using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInitializeEvent{
    
    public readonly Transform Identifier;
    
    public PlayerInitializeEvent(Transform identifier)
    {   
        Debug.Log("Event 注册玩家标识符" + identifier.name);
        this.Identifier = identifier;
    }
}
public class PlayerDieEvent{

}
public class PlayerRegister : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform identifier;

    private void Start()
    {
        if(identifier == null)
        {
            identifier = this.transform.Find("PlayerTarget");
        }
        EventBus<PlayerInitializeEvent>.Publish(new PlayerInitializeEvent(identifier));
    }

    private void OnDestroy()
    {
        EventBus<PlayerDieEvent>.Publish(new PlayerDieEvent());
    }
}
