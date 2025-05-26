using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private Animator animator;

    [Header("References")]
    [SerializeField] SceneLoader loader;
    [SerializeField] private Button next;
    [SerializeField] private Button prev;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI tutorial;
    [SerializeField] private TextMeshProUGUI level1;
    [SerializeField] private TextMeshProUGUI level2;
    [SerializeField] private TextMeshProUGUI level3;

    private int levelIndex = 0;

    private void Start()
    {
        Time.timeScale = 1f;
        animator = GetComponent<Animator>();
        animator.ResetTrigger("IdleToLevelSelect");
        animator.ResetTrigger("LevelSelectToIdle");
    }

    private void Update()
    {
        next.interactable = levelIndex < 3;
        prev.interactable = levelIndex > 0;

        tutorial.color = levelIndex == 0 ? Color.white : Color.clear;
        level1.color = levelIndex == 1 ? Color.white : Color.clear;
        level2.color = levelIndex == 2 ? Color.white : Color.clear;
        level3.color = levelIndex == 3 ? Color.white : Color.clear;
    }

    public void GoToLevelSelectScreen()
    {
        animator.SetTrigger("IdleToLevelSelect");
    }

    public void GoToIdleScreen()
    {
        animator.SetTrigger("LevelSelectToIdle");
    }

    public void NextButton()
    {
        if (levelIndex < 3)
        {
            levelIndex++;
        }
    }

    public void PrevButton()
    {
        if (levelIndex > 0)
        {
            levelIndex--;
        }
    }

    public void PlayLevel()
    {
        switch (levelIndex)
        {
            case 0:
                loader.LoadTutorial();
                break;
            case 1:
                //loader.LoadLevel1();
                break;
            case 2:
                loader.LoadLevel2();
                break;
            case 3:
                //loader.LoadLevel3();
                break;
        }
    }
}
