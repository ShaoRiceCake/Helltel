using UnityEngine;

public class StableBodyAndHead : MonoBehaviour
{
    public GameObject foot1; // 脚1对象
    public GameObject foot2; // 脚2对象
    public GameObject head;  // 头对象

    [Header("高度控制参数")]
    public float baseHeight = 1.0f;      // 基础高度偏移
    public float logFactor = 0.5f;       // 对数缩放系数

    void Update()
    {
        Vector3 foot1Pos = foot1.transform.position;
        Vector3 foot2Pos = foot2.transform.position;

        // 计算基础几何参数
        Vector3 footVector = foot2Pos - foot1Pos;
        float footDistance = footVector.magnitude;
        Vector3 midpoint = (foot1Pos + foot2Pos) * 0.5f;
        float maxFootY = Mathf.Min(foot1Pos.y, foot2Pos.y);


        // 计算头部目标位置
        Vector3 headPosition = midpoint + Vector3.up ;


        // 应用最终位置
        head.transform.position = headPosition;
    }
}