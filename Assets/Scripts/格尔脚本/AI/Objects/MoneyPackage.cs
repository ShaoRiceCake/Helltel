using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPackage : MonoBehaviour
{
    // Start is called before the first frame update

    public int MoneyValue = 1; // 金钱包的价值
    
    public void Init(int value)
    {
        MoneyValue = value;
    }




}
