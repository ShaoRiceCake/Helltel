using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;

public class MothGroupController : GuestBase
{
    private List<MothController> mothList = new List<MothController>(); //虫群列表



    public void RegisterMoth(GameObject moth)
    {
        MothController mothController = moth.GetComponent<MothController>();
        if (mothController == null)
        {
            if(Debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }
        if (!mothList.Contains(mothController))
        {
            mothList.Add(mothController);
            mothController.belongToGroup = this; //设置所属虫群
        }
    }
    public void UnregisterMoth(GameObject moth)
    {
        MothController mothController = moth.GetComponent<MothController>();
        if (mothController == null)
        {
            if(Debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }
        if (mothList.Contains(mothController))
        {
            mothList.Remove(mothController);
            mothController.belongToGroup = null; //设置所属虫群为空
        }
    }
}
