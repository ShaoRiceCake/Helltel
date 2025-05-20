using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Helltel/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("基本信息")]
    public string monsterName;
    public Sprite monsterImage;

    [Header("图鉴内容")]
    [TextArea(3, 10)] public string registryText;
    [TextArea(3, 10)] public string observationText;
    [TextArea(3, 10)] public string solutionText;

}