using TMPro;
using UnityEngine;

public class AdventurerUI : MonoBehaviour
{
    public static AdventurerUI Instance { get; private set; }

    [Header("UI组件")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject techTree;
    [SerializeField] private GameObject order;
    [SerializeField] private GameObject data;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateTimer(float time)
    {
        timerText.text = $"Time: {Mathf.Max(time,0):F1}s";
    }

    public void UpdateCoin(float coin)
    {
        coinsText.text = $"Coins: {DataManager.Instance.coinCount:F0}";
    }

    public void ShowTechTree()
    {
        order.SetActive(false);
        data.SetActive(false);
        techTree.SetActive(true);
    }

    public void ShowOrder()
    {
        data.SetActive(false);
        techTree.SetActive(false);
        order.SetActive(true);
    }

    public void ShowData()
    {
        order.SetActive(false);
        techTree.SetActive(false);
        data.SetActive(true);
    }
    
}
