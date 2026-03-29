using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TechNodeUI : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public int id;
    public Button button;
    public Image icon;

    private TechTreeManager manager;
    private TechNodeData data;
    private bool isUnlocked = false;
    private bool isAvailable = false;

    public void Init(int id, TechTreeManager manager)
    {
        this.id = id;
        this.manager = manager;
        this.data = manager.GetNodeData(id);

        isUnlocked = false;
        isAvailable = false;

        button.onClick.AddListener(OnClick);

        UpdateVisual();
    }

    public void SetAvailable(bool value)
    {
        
        isAvailable = value;
        UpdateVisual();
    }

    public void SetUnlocked()
    {
        isUnlocked = true;
        isAvailable = false;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (isUnlocked)
        {
            icon.color = Color.green; // 已解锁
        }
        else if (isAvailable)
        {
            icon.color = Color.white; // 可解锁
        }
        else
        {
            icon.color = Color.red; // 未开放
        }
    }

    void OnClick()
    {
        if (isAvailable)
        {
            manager.TryUnlock(id);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        string content =
            $"<b>消耗：</b>{data.costGold}金币 / {data.costTech}科技\n" +
            $"{data.description}";

        TooltipManager.Instance.Show(content);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.Hide();
    }
}