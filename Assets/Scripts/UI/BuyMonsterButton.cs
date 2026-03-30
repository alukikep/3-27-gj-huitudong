using UnityEngine;
using UnityEngine.UI;

public class BuyMonsterButton : MonoBehaviour
{
    public MonsterType monsterType;
    public float cost = 10;

    public GameObject monsterPrefab;

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
}