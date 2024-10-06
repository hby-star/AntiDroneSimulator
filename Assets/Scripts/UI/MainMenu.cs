using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button startGameButton;
    public Button helpButton;
    public Button exitGameButton;
    public Button settingsButton;

    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        helpButton.onClick.AddListener(Help);
        exitGameButton.onClick.AddListener(ExitGame);
        settingsButton.onClick.AddListener(Settings);
    }

    private void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    private void Help()
    {
        UIManager.Instance.ShowHelpPopUp();
    }

    private void ExitGame()
    {
        GameManager.Instance.QuitGame();
    }

    private void Settings()
    {
        UIManager.Instance.ShowSettings();
    }
}
