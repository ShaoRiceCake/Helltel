using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemController : MonoBehaviour
{
    [HideInInspector] public bool isInRoom = true;
    private Vector3 lastRoomPosition;
    
    void Start()
    {
        lastRoomPosition = transform.position;
    }

    public void UpdateRoomStatus(bool insideRoom)
    {
        isInRoom = insideRoom;
        if (isInRoom)
        {
            lastRoomPosition = transform.position;
        }
    }

    public void ReturnToRoom()
    {
        transform.position = lastRoomPosition;
        isInRoom = true;
    }
    
}