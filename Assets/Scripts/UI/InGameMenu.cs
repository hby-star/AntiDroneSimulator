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
        Messenger.Broadcast(UIEvent.HIDE_ALL_POPUPS);
        Messenger.Broadcast(GameEvent.CONTINUE_GAME);
    }

    void Help()
    {
        Messenger.Broadcast(UIEvent.SHOW_HELP);
    }

    void Return()
    {
        Messenger.Broadcast(GameEvent.TO_MAIN_MENU);
    }
}
