using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    //area1 休息区
    //area2 轻工厂
    //area3 重工厂
    //area4 地狱工厂
    public SpriteRenderer area3SpriteRenderer;
    public SpriteRenderer area4SpriteRenderer;
    public Sprite area3Sprite;
    public Sprite area4Sprite;
    //后续有需求可以用SO填数据
    [Header("区域数据")]
    public float area1Time;//冒险者在每个区域的工作效率，可以设置为获取金币时间间隔或者获取金币倍率，我个人倾向于获取金币时间间隔
    public float area1Damage;
    public float area2Time;
    public float area2Damage;
    public float area3Time;
    public float area3Damage;
    public float area4Time;
    public float area4Damage;
    [Header("冒险者数据")]
    public float adventurerEfficiency;//冒险者效率，可以设置为每隔一段时间获得的金币数
    public float coinCount;//当前拥有的金币数
    public float GameTime;
    public float buyBonus = 1;
    [Header("怪物限制数数据")]
    public int goblinMaxCount;
    public int goblinCurrentCount;
    public int slimeMaxCount;
    public int slimeCurrentCount;
    public int trollMaxCount;
    public int trollCurrentCount;
    public int succubusMaxCount;
    public int succubusCurrentCount;
    public int skeletonMaxCount;
    public int skeletonCurrentCount;

    [Header("区域单位数量限制")]
    public int area1MaxCount = int.MaxValue;  // 休息区 - 无限容量
    public int area1CurrentCount;
    public int area2MaxCount = 4;  // 轻松区
    public int area2CurrentCount;
    public int area3MaxCount = 0;  // 普通区
    public int area3CurrentCount;
    public int area4MaxCount = 0;  // 困难区
    public int area4CurrentCount;
    [Header("解锁相关")]
    public bool isArea3Unlocked;
    public bool isArea4Unlocked;
    public bool isGoblinUnlocked;
    public bool isSlimeUnlocked;
    public bool isTrollUnlocked;
    public bool isSuccubusUnlocked;
    public bool isSkeletonUnlocked;

    [Header("休息区位置")]
    public Transform restAreaPosition;  // 休息区位置，拖动Transform赋值
    public float castleMaxHealth = 1200; // 魔王城初始血量
    public float castleHealth = 1200; // 魔王城血量 
    public float castleDamage = 1;
    public float targetCoinCount = 3000; // 胜利条件：达到3000金币

    void OnEnable()
    {
        EventManager.AddListener<int>("UnlockTechTree", TechTreeEffect);

    }

    void OnDisable()
    {
        EventManager.RemoveListener<int>("UnlockTechTree", TechTreeEffect);
    }

    void Update()
    {
        GameTime += Time.deltaTime;
        AdventurerUI.Instance.UpdateTimer(GameTime);
        AdventurerUI.Instance.UpdateCoin(coinCount);
        castleHealth -= Time.deltaTime * castleDamage; // 假设每秒减少5点血量，实际可以根据怪物数量和类型调整
        if (castleHealth <= 0)
        {
            GameLose();
        }
        GameWin();
    }

    public void AdventurerUpgrade(float amount)//可以升级时调用也可以解锁科技树提高工人效率时调用
    {
        adventurerEfficiency += amount;
    }

    public void AreaUpgrade(int areaIndex, float amount)//提高工作区效率的方法，乘算减少时间间隔，看情况实现
    {

    }

    public void ChangeCoins(float amount)
    {
        coinCount += amount;
        //UI更新逻辑
    }

    /// <summary>
    /// 检查指定区域是否可以放置单位
    /// </summary>
    /// <param name="areaIndex">区域索引(1-4)</param>
    /// <returns>是否可以放置</returns>
    public bool CanPlaceInArea(int areaIndex)
    {
        // 对于区域3和4，需要先检查是否解锁
        if (areaIndex == 3 && !isArea3Unlocked)
        {
            return false;
        }
        if (areaIndex == 4 && !isArea4Unlocked)
        {
            return false;
        }

        int currentCount = 0;
        int maxCount = 0;

        switch (areaIndex)
        {
            case 1:
                currentCount = area1CurrentCount;
                maxCount = area1MaxCount;
                break;
            case 2:
                currentCount = area2CurrentCount;
                maxCount = area2MaxCount;
                break;
            case 3:
                currentCount = area3CurrentCount;
                maxCount = area3MaxCount;
                break;
            case 4:
                currentCount = area4CurrentCount;
                maxCount = area4MaxCount;
                break;
            default:
                return false;
        }

        return currentCount < maxCount;
    }

    /// <summary>
    /// 增加指定区域的单位计数
    /// </summary>
    public void IncrementAreaCount(int areaIndex)
    {
        switch (areaIndex)
        {
            case 1: area1CurrentCount++; break;
            case 2: area2CurrentCount++; break;
            case 3: area3CurrentCount++; break;
            case 4: area4CurrentCount++; break;
        }
    }

    /// <summary>
    /// 减少指定区域的单位计数
    /// </summary>
    public void DecrementAreaCount(int areaIndex)
    {
        switch (areaIndex)
        {
            case 1: area1CurrentCount = Mathf.Max(0, area1CurrentCount - 1); break;
            case 2: area2CurrentCount = Mathf.Max(0, area2CurrentCount - 1); break;
            case 3: area3CurrentCount = Mathf.Max(0, area3CurrentCount - 1); break;
            case 4: area4CurrentCount = Mathf.Max(0, area4CurrentCount - 1); break;
        }
    }

    /// <summary>
    /// 根据状态获取区域索引
    /// </summary>
    public static int GetAreaIndexFromState(adventurerState state)
    {
        switch (state)
        {
            case adventurerState.REST: return 1;
            case adventurerState.WORKEASY: return 2;
            case adventurerState.WORK: return 3;
            case adventurerState.WORKHARD: return 4;
            default: return 1;
        }
    }


    private void TechTreeEffect(int id)
    {
        switch (id)
        {
            case 0:
                area4Time *= 0.8f;
                break;
            case 1:
                castleMaxHealth += 300;
                castleHealth += 300;
                break;
            case 2:
                area4MaxCount += 2;
                break;
            case 3:
                //解锁巨魔
                isTrollUnlocked = true;
                trollMaxCount += 1;
                break;
            case 4:
                //解锁area4
                isArea4Unlocked = true;
                area4SpriteRenderer.sprite = area4Sprite; // 切换到地狱工厂的背景图
                area4MaxCount += 4;
                break;
            case 5:
                area3Time *= 0.8f;
                break;
            case 6:
                area3MaxCount += 2;
                break;
            case 7:
                area2Time *= 0.8f;
                break;
            case 8:
                goblinMaxCount += 2;
                trollMaxCount++;
                skeletonMaxCount++;
                succubusMaxCount++;
                slimeMaxCount += 2;
                break;
            case 9:
                area1Damage *= 1.05f;
                break;
            case 10:
                //初始解锁哥布林
                break;
            case 11:
                //解锁熔炉
                isArea3Unlocked = true;
                area3SpriteRenderer.sprite = area3Sprite; // 切换到重工厂的背景图
                area3MaxCount += 4;
                break;
            case 12:
                //解锁史莱姆
                isSlimeUnlocked = true;
                slimeMaxCount += 4;
                break;
            case 13:
                area2MaxCount += 2;
                break;
            case 14:
                //购买怪物需要的金币减少
                buyBonus = 0.8f;
                break;
            case 15:
                area2Time *= 0.95f;
                area3Time *= 0.95f;
                area4Time *= 0.95f;
                break;
            case 16:
                area2Damage *= 0.95f;
                area3Damage *= 0.95f;
                area4Damage *= 0.95f;
                break;
            case 17:
                //解锁魅魔
                isSuccubusUnlocked = true;
                succubusMaxCount += 1;
                break;
            case 18:
                //解锁骷髅
                isSkeletonUnlocked = true;
                skeletonMaxCount += 1;

                break;
            case 19:
                area2Damage *= 0.9f;
                area3Damage *= 0.9f;
                area4Damage *= 0.9f;
                break;
            case 20:
                //降低魔王城血条减少速度
                castleDamage *= 0.7f;
                break;
        }
    }
    public bool TryBuyMonster(MonsterType type, float cost)
    {
        // 1. 是否解锁
        if (!IsMonsterUnlocked(type))
        {
            Debug.Log("未解锁该怪物");
            return false;
        }

        // 2. 钱够不够
        if (coinCount < cost * buyBonus)
        {
            Debug.Log("金币不足");
            return false;
        }

        // 3. 是否超过数量限制
        if (!CanSpawnMonster(type))
        {
            Debug.Log("数量已满");
            return false;
        }

        // 4. 扣钱
        ChangeCoins(-cost * buyBonus);

        // 5. 增加数量
        IncrementMonsterCount(type);

        return true;
    }

    private bool IsMonsterUnlocked(MonsterType type)
    {
        return type switch
        {
            MonsterType.Goblin => isGoblinUnlocked,
            MonsterType.Slime => isSlimeUnlocked,
            MonsterType.Troll => isTrollUnlocked,
            MonsterType.Succubus => isSuccubusUnlocked,
            MonsterType.Skeleton => isSkeletonUnlocked,
            _ => false
        };
    }

    private bool CanSpawnMonster(MonsterType type)
    {
        return type switch
        {
            MonsterType.Goblin => goblinCurrentCount < goblinMaxCount,
            MonsterType.Slime => slimeCurrentCount < slimeMaxCount,
            MonsterType.Troll => trollCurrentCount < trollMaxCount,
            MonsterType.Succubus => succubusCurrentCount < succubusMaxCount,
            MonsterType.Skeleton => skeletonCurrentCount < skeletonMaxCount,
            _ => false
        };
    }

    private void IncrementMonsterCount(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Goblin: goblinCurrentCount++; break;
            case MonsterType.Slime: slimeCurrentCount++; break;
            case MonsterType.Troll: trollCurrentCount++; break;
            case MonsterType.Succubus: succubusCurrentCount++; break;
            case MonsterType.Skeleton: skeletonCurrentCount++; break;
        }
    }

    private void GameWin()
    {
        if (coinCount >= targetCoinCount) // 假设3000金币为胜利条件
        {
            Debug.Log("游戏胜利！");
            // 这里可以添加胜利界面或其他逻辑
        }
    }

    private void GameLose()
    {

        // 这里可以添加失败条件和逻辑，例如冒险者死亡或时间耗尽
        Debug.Log("游戏失败！");
        // 这里可以添加失败界面或其他逻辑
    }
}
public enum MonsterType
{
    Goblin,
    Slime,
    Troll,
    Succubus,
    Skeleton
}
