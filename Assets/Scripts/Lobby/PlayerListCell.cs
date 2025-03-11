using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class PlayerListCell : MonoBehaviour
{
    public TMP_Text _name;
    public TMP_Text _ready;
    public PlayerInfo PlayerInfo { get; private set; }

    public void Initial(PlayerInfo playerInfo)
    {
        PlayerInfo = playerInfo;
        _name.text = playerInfo.name;
        _ready.text = playerInfo.isReady ? "��׼��" : "δ׼��";
    }

    public void UpdateInfo(PlayerInfo playerInfo)
    {
        PlayerInfo = playerInfo;
        _name.text = playerInfo.name;
        _ready.text = PlayerInfo.isReady ? "��׼��" : "δ׼��";
    }


    internal void SetReady(bool arg0)
    {
        _ready.text = arg0 ? "��׼��" : "δ׼��";
    }
}
