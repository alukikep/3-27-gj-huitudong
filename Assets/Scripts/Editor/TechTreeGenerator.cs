using UnityEngine;
using UnityEditor;

public class TechTreeGenerator
{
    [MenuItem("Tools/Generate Tech Tree")]
    public static void Generate()
    {
        string path = "Assets/TechNodes/";

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "TechNodes");
        }

        int id = 0;

        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                TechNodeData node = ScriptableObject.CreateInstance<TechNodeData>();

                node.id = id;
                node.position = new Vector2Int(x, y);
                node.costGold = 100;
                node.costTech = 10;
                node.unlockedByDefault = (x == 2 && y == 2);

                AssetDatabase.CreateAsset(node, $"{path}Node_{id}.asset");

                id++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("科技树生成完成！");
    }
}