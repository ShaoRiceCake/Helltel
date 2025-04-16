using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneItemManager : MonoBehaviour
{
    public Collider roomBoundary;
    public List<ItemController> allItems = new List<ItemController>();

    void Update()
    {
        foreach (var item in allItems)
        {
            bool isInside = roomBoundary.bounds.Contains(item.transform.position);
            if (isInside != item.isInRoom)
            {
                item.UpdateRoomStatus(isInside);
            }
        }
    }

    public void RegisterItem(ItemController item)
    {
        if (!allItems.Contains(item))
        {
            allItems.Add(item);
            item.UpdateRoomStatus(roomBoundary.bounds.Contains(item.transform.position));
        }
    }

    public void UnregisterItem(ItemController item)
    {
        if (allItems.Contains(item))
        {
            allItems.Remove(item);
        }
    }
}