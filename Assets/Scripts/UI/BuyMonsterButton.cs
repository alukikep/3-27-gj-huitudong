using UnityEngine;
using UnityEngine.UI;

public class BuyMonsterButton : MonoBehaviour
{
    public MonsterType monsterType;
    public float cost = 10;

    public GameObject monsterPrefab;
    public Image icon;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    public void OnClickBuy()
    {
        bool success = DataManager.Instance.TryBuyMonster(monsterType, cost);

        if (success)
        {
            SpawnMonster();
        }
    }

    void SpawnMonster()
    {
        AudioManager.Instance.PlaySFX("Buy", true);
        Instantiate(monsterPrefab, DataManager.Instance.restAreaPosition.position, Quaternion.identity);
    }

    void Update()
    {
        if (monsterType != MonsterType.Goblin)
        {
            UpdateUI();
        }
        
    }
    
    public void UpdateUI()
    {
        bool isUnlocked = IsUnlocked();

        icon.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        
    }
    
    bool IsUnlocked()
    {
        var dm = DataManager.Instance;

        return monsterType switch
        {
            MonsterType.Goblin => dm.isGoblinUnlocked,
            MonsterType.Slime => dm.isSlimeUnlocked,
            MonsterType.Troll => dm.isTrollUnlocked,
            MonsterType.Succubus => dm.isSuccubusUnlocked,
            MonsterType.Skeleton => dm.isSkeletonUnlocked,
            _ => false
        };
    }
}