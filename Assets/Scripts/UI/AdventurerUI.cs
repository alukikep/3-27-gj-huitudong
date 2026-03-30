using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerUI : MonoBehaviour
{
    public static AdventurerUI Instance { get; private set; }

    [Header("UI组件")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject techTree;
    [SerializeField] private GameObject order;
    [SerializeField] private GameObject data;
    [SerializeField] private Slider castleSlider;
    [SerializeField] private float startX;
    [SerializeField] private float endX;
    [SerializeField] private RectTransform marker;
    
    public GameObject winPanel;
    public GameObject losePanel;
    private bool isGameEnd = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {

        UpdateCastleHP();
        UpdateMarker();


        if (isGameEnd)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ReturnToMenu();
            }
        }
    }

    void UpdateCastleHP()
    {
        Debug.Log("更新血条: " + DataManager.Instance.castleHealth);
        float current = DataManager.Instance.castleHealth;
        float max = DataManager.Instance.castleMaxHealth;
        castleSlider.value = Mathf.Clamp01(current / max);
    }
    
    void UpdateMarker()
    {
        float current = DataManager.Instance.coinCount;
        float max = DataManager.Instance.targetCoinCount;

        if (max <= 0) return;

        float percent = Mathf.Clamp01(current / max);

        float x = Mathf.Lerp(startX, endX, percent);

        marker.anchoredPosition = new Vector2(x, marker.anchoredPosition.y);
    }

    public void UpdateTimer(float time)
    {
        timerText.text = $"Time: {Mathf.Max(time,0):F1}s";
    }

    public void UpdateCoin(float coin)
    {
        coinsText.text = $": {DataManager.Instance.coinCount:F0}";
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
    public void ShowWin()
    {
        isGameEnd = true;

        Time.timeScale = 0f;
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        isGameEnd = true;

        Time.timeScale = 0f;
        losePanel.SetActive(true);
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f; // ⚠️必须恢复

        SceneLoader.Instance.LoadScene("StartScene"); // 改成你的主菜单场景名
    }
}

