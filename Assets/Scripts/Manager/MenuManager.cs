using UnityEngine;

public class MenuManager : MonoBehaviour
{

    void Start()
    {
        AudioManager.Instance.PlayMusic("StartMenu");
    }
    public void StartGame()
    {
        SceneLoader.Instance.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}