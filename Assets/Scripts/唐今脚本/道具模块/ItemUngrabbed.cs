using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemUngrabbed : MonoBehaviour, IInteractable
{
    [SerializeField] protected string itemName; 
    public string ItemName => itemName;
    public EItemState CurrentState { get; }
    [SerializeField] protected int itemDamage;  
    public int ItemDamage
    {
        get => itemDamage;
        set => itemDamage = value;
    }
    public ItemUngrabbed(EItemState currentState)
    {
        CurrentState = currentState;
    }
}
