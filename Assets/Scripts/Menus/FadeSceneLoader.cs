using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeSceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Button button;
    [SerializeField] public string sceneName;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool fadeInOnStart = true;

    private bool isChangingScene;

    private void Awake()
    {
        if (fadeCanvasGroup == null)
        {
            return;
        }

        if (fadeInOnStart)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (fadeInOnStart && fadeCanvasGroup != null)
        {
            StartCoroutine(FadeFromBlack());
        }
    }

    public void FadeAndLoadScene()
    {
        FadeAndLoadScene(sceneName);
    }

    public void FadeAndLoadScene(string targetSceneName)
    {
        if (isChangingScene)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("No se ha asignado ninguna escena para cargar.", this);
            return;
        }

        sceneName = targetSceneName;
        isChangingScene = true;

        if (button != null)
        {
            button.interactable = false;
        }

        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeFromBlack()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            }

            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene(sceneName);
    }
}
