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

    private void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        helpButton.onClick.AddListener(Help);
        exitGameButton.onClick.AddListener(ExitGame);
    }

    private void StartGame()
    {
        Messenger.Broadcast(GameEvent.START_GAME);
    }

    private void Help()
    {
        Messenger.Broadcast(UIEvent.SHOW_HELP);
    }

    private void ExitGame()
    {
        Messenger.Broadcast(GameEvent.EXIT_GAME);
    }
}
