using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class MainMenu : MonoBehaviour
{
    public Button startGameButton;
    public Button exitGameButton;

    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        exitGameButton.onClick.AddListener(ExitGame);
    }

    private void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    private void ExitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
