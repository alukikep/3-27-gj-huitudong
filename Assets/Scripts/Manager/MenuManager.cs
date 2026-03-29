using UnityEngine;

public class MenuManager : MonoBehaviour
{
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