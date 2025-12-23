using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Page[] pages;
    private Animator animator;
    public GameObject play;

    private int currentPage = 0;
    private bool isAnimating = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isAnimating) UpdateVisibleCanvases();
    }

    public void MainPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toMain");
        currentPage = 0;
    }

    public void SettingsPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toSettings");
        currentPage = 1;
    }

    public void AchievementsPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toAchievements");
        currentPage = 2;
    }

    public void ExtrasPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toExtras");
        currentPage = 3;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void OnPageAnimationFinished()
    {
        isAnimating = false;
        UpdateVisibleCanvases();
    }

    private void ActivateAllCanvases()
    {
        foreach (var page in pages) page.SetBoth(true);
    }

    private void UpdateVisibleCanvases()
    {
        for (int i = 0; i < pages.Length; i++) pages[i].SetBoth(false);

        if (currentPage - 1 >= 0) pages[currentPage - 1].SetBack(true);

        if (currentPage < pages.Length) pages[currentPage].SetFront(true);

        play.SetActive(currentPage == 0);
    }
}