using System.Collections.Generic;
using UnityEngine;

public class TechTreeManager : MonoBehaviour
{
    public int width = 3;
    public int height = 7;

    public TechNodeData[] allNodes;    // 25 个节点
    public TechNodeUI[] nodeUIs;       // 对应 UI，长度必须 = allNodes.Length

    private Dictionary<int, TechNodeData> nodeDict = new();
    private Dictionary<Vector2Int, int> grid = new();
    private HashSet<int> unlockedNodes = new();

    public float  playerGold ;
    /*public int playerTech = 100;*/

    void Start()
    {
        playerGold = DataManager.Instance.coinCount;
        Init();
    }

    void Init()
    {
        // 1️⃣ 建立字典和位置映射
        for (int i = 0; i < allNodes.Length; i++)
        {
            var node = allNodes[i];
            nodeDict[node.id] = node;
            grid[node.position] = node.id;
        }

        // 2️⃣ 初始化 UI
        for (int i = 0; i < nodeUIs.Length; i++)
        {
            nodeUIs[i].Init(allNodes[i].id, this);
        }

        // 3️⃣ 解锁默认节点
        for (int i = 0; i < allNodes.Length; i++)
        {
            if (allNodes[i].unlockedByDefault)
            {
                UnlockNode(allNodes[i].id, true);
            }
        }
    }

    // 尝试解锁节点（消耗资源）
    public void TryUnlock(int id)
    {
        var node = nodeDict[id];

        if (playerGold < node.costGold )
        {
            Debug.Log("资源不足");
            AudioManager.Instance.PlaySFX("TechCantUnlock", true);
            return;
        }

        playerGold -= node.costGold;

        UnlockNode(id, false);
    }

    // 真正解锁
    void UnlockNode(int id, bool isInit)
    {
        if (unlockedNodes.Contains(id)) return;

        unlockedNodes.Add(id);

        // 更新 UI
        GetUI(id)?.SetUnlocked();
        AudioManager.Instance.PlaySFX("TechUnlock", true);
        EventManager.Broadcast("UnlockTechTree", id);//添加事件广播

        // 刷新周围节点状态
        UpdateNeighbors(id);
    }

    // 开放四周节点
    void UpdateNeighbors(int id)
    {
        if (!nodeDict.ContainsKey(id)) return;

        Vector2Int pos = nodeDict[id].position;
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector2Int target = pos + dir;

            if (!grid.ContainsKey(target)) continue;

            int neighborId = grid[target];

            // 只开放未解锁节点
            if (!unlockedNodes.Contains(neighborId))
            {
                var ui = GetUI(neighborId);
                if (ui != null)
                {
                    ui.SetAvailable(true);
                }
            }
        }
    }

    // 根据 id 获取 UI
    TechNodeUI GetUI(int id)
    {
        foreach (var ui in nodeUIs)
        {
            if (ui.id == id) return ui;
        }
        return null;
    }

    // 外部获取已解锁节点
    public List<int> GetUnlockedIDs()
    {
        return new List<int>(unlockedNodes);
    }
    public TechNodeData GetNodeData(int id)
    {
        return nodeDict.ContainsKey(id) ? nodeDict[id] : null;
    }
}