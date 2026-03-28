using UnityEngine;
using UnityEngine.UI;

public class TechNodeUI : MonoBehaviour
{
    public int id;
    public Button button;
    public Image icon;

    private TechTreeManager manager;
    private bool isUnlocked = false;
    private bool isAvailable = false;

    public void Init(int id, TechTreeManager manager)
    {
        this.id = id;
        this.manager = manager;

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
}