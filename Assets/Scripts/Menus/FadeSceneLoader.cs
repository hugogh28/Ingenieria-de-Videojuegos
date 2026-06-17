using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeSceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Button button;
    [SerializeField] private string sceneName;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool fadeInOnStart = true;

    private bool _isChangingScene;

    private void Awake()
    {
        if (fadeInOnStart)
        {
            fadeCanvasGroup.alpha = 1;
            fadeCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (fadeInOnStart) StartCoroutine(FadeFromBlack());
    }

    public void FadeAndLoadScene()
    {
        if (_isChangingScene) return;
        _isChangingScene = true;

        if (button != null) button.interactable = false;

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
        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration) 
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        SceneManager.LoadScene(sceneName);
    }
}
