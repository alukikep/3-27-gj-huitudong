using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject panel;
    public Text text;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        if (panel.activeSelf)
        {
            Vector3 offset = new Vector3(140, -140, 0); 
            panel.transform.position = Input.mousePosition + offset;
        }
    }

    public void Show(string content)
    {
        text.text = content;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}