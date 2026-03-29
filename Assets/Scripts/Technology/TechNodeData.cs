using UnityEngine;

[CreateAssetMenu(fileName = "TechNode", menuName = "TechTree/Node")]
public class TechNodeData : ScriptableObject
{
    public int id;
    public int costGold;
    public int costTech;

    public Vector2Int position; 

    public bool unlockedByDefault;
    [TextArea]
    public string description;
}