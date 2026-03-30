using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("教程图片")]
    public Sprite[] tutorialSprites; // 所有教程图片
    public Image tutorialImage;      // UI Image 显示当前页

    private int currentIndex = 0;
    private bool isTutorialActive = true;

    void Start()
    {
        if (tutorialSprites.Length == 0)
        {
            Debug.LogWarning("Tutorial sprites not set!");
            EndTutorial();
            return;
        }

        // 第一次显示第一张图片
        tutorialImage.sprite = tutorialSprites[0];
        tutorialImage.transform.parent.gameObject.SetActive(true); // 确保Panel显示

        // 暂停游戏
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!isTutorialActive) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            NextImage();
        }
    }

    void NextImage()
    {
        currentIndex++;

        if (currentIndex >= tutorialSprites.Length)
        {
            EndTutorial();
            return;
        }

        tutorialImage.sprite = tutorialSprites[currentIndex];
    }

    void EndTutorial()
    {
        isTutorialActive = false;

        // 隐藏整个Panel
        tutorialImage.transform.parent.gameObject.SetActive(false);

        // 恢复游戏
        Time.timeScale = 1f;

        // 调用正式游戏开始逻辑
        DataManager.Instance.StartGame();
    }
}