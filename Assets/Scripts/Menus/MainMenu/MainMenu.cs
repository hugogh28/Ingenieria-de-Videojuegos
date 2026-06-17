using System;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Page[] pages;
    private Animator animator;

    public Animator burger;
    public Animator potato;
    public Animator drink;

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
        if (burger.GetBool("burger")) burger.SetBool("burger", false);
        if (potato.GetBool("potato")) potato.SetBool("potato", false);
        if (drink.GetBool("drink")) drink.SetBool("drink", false);
        currentPage = 0;
    }

    public void SettingsPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toSettings");
        burger.SetBool("burger", true);
        currentPage = 1;
    }

    public void AchievementsPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toAchievements");
        potato.SetBool("potato", true);
        currentPage = 2;
    }

    public void ExtrasPage()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toExtras");
        potato.SetBool("potato", true);
        burger.SetBool("burger", true);
        drink.SetBool("drink", true);
        currentPage = 3;
    }

    public void toCredits()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("toCredits");
        if (burger.GetBool("burger")) burger.SetBool("burger", false);
        if (potato.GetBool("potato")) potato.SetBool("potato", false);
        if (drink.GetBool("drink")) drink.SetBool("drink", false);
        currentPage = 0;
    }

    public void Quit()
    {
        if (isAnimating) return;

        isAnimating = true;
        ActivateAllCanvases();
        animator.SetTrigger("Quit");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
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