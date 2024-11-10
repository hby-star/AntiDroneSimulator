using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public Button continueButton;
    public Button returnButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
        returnButton.onClick.AddListener(Return);
    }

    void Continue()
    {
        UIManager.Instance.HideGamePausePopUp();
        GameManager.Instance.ContinueGame();
    }

    void Return()
    {
        GameManager.Instance.ToMainMenu();
    }
}