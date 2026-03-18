using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    [Header("黑屏淡入淡出时间")]
    public float fadeDuration = 0.5f;

    [Header("黑屏等待时间")]
    public float waitDuration = 0.5f;

    private CanvasGroup fadeCanvasGroup;
    private Image fadeImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupFadeUI()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        GameObject imageObj = new GameObject("BlackScreen");
        imageObj.transform.SetParent(canvasObj.transform, false);
        imageObj.AddComponent<RectTransform>().anchorMin = Vector2.zero;
        imageObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        imageObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
    }

    public void LoadScene(string sceneName)
    {
        if (fadeCanvasGroup == null)
        {
            SetupFadeUI();
        }
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    public void ReloadScene()
    {
        if (fadeCanvasGroup == null)
        {
            SetupFadeUI();
        }
        StartCoroutine(TransitionCoroutine(SceneManager.GetActiveScene().name));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(waitDuration);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (op.progress < 0.9f)
        {
            yield return null;
        }
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}

