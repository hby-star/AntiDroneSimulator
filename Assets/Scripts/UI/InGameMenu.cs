using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public Button continueButton;
    public Button helpButton;
    public Button returnButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
        helpButton.onClick.AddListener(Help);
        returnButton.onClick.AddListener(Return);
    }

    void Continue()
    {
        UIManager.Instance.HideGamePausePopUp();
        GameManager.Instance.ContinueGame();
    }

    void Help()
    {
        UIManager.Instance.ShowHelpPopUp();
    }

    void Return()
    {
        GameManager.Instance.ToMainMenu();
    }
}