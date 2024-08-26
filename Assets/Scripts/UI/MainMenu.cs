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
    public GameObject helpPopUp;

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
        helpPopUp.SetActive(true);
    }

    private void ExitGame()
    {
        Messenger.Broadcast(GameEvent.EXIT_GAME);
    }

    void Start()
    {
        helpPopUp.SetActive(false);
    }

    void Update()
    {
        if (helpPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(helpPopUp.GetComponent<RectTransform>(), Input.mousePosition))
                {
                    helpPopUp.SetActive(false);
                }
            }
        }
    }
}
