using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("暂停界面")]
    public GameObject pauseUI;

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }
    

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseUI.SetActive(false);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("StartScene");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}