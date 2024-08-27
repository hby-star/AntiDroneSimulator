using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Singleton
    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    _instance = obj.AddComponent<UIManager>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public GameObject helpPopUp;
    public GameObject gameStartPopUp;
    public GameObject gameEndPopUp;

    public void HideAllPopUps()
    {
        helpPopUp.SetActive(false);
        gameStartPopUp.SetActive(false);
        gameEndPopUp.SetActive(false);
    }

    public void ShowHelpPopUp()
    {
        helpPopUp.SetActive(true);
    }

    public void HideHelpPopUp()
    {
        helpPopUp.SetActive(false);
    }

    public void ShowGameStartPopUp()
    {
        gameStartPopUp.SetActive(true);
    }

    public void HideGameStartPopUp()
    {
        gameStartPopUp.SetActive(false);
    }

    public void ShowGameEndWinPopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Win!";
        gameEndPopUp.SetActive(true);
    }

    public void ShowGameEndLosePopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Lose!";
        gameEndPopUp.SetActive(true);
    }

    public void HideGameEndPopUp()
    {
        gameEndPopUp.SetActive(false);
    }

    void Start()
    {
        HideAllPopUps();
    }

    void Update()
    {
        if (gameStartPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HideGameStartPopUp();
                Messenger.Broadcast(GameEvent.CONTINUE_GAME);
            }
        }

        if (gameEndPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HideGameEndPopUp();
                Messenger.Broadcast(GameEvent.TO_MAIN_MENU);
            }
        }

        if (helpPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        helpPopUp.transform.GetChild(0).GetComponent<RectTransform>(), Input.mousePosition))
                {
                    HideHelpPopUp();
                }
            }
        }
    }

    private void OnEnable()
    {
        Messenger.AddListener(UIEvent.SHOW_HELP, ShowHelpPopUp);
        Messenger.AddListener(UIEvent.HIDE_HELP, HideHelpPopUp);
        Messenger.AddListener(UIEvent.SHOW_GAME_START, ShowGameStartPopUp);
        Messenger.AddListener(UIEvent.HIDE_GAME_START, HideGameStartPopUp);
        Messenger.AddListener(UIEvent.SHOW_GAME_END_WIN, ShowGameEndWinPopUp);
        Messenger.AddListener(UIEvent.SHOW_GAME_END_LOSE,ShowGameEndLosePopUp);
        Messenger.AddListener(UIEvent.HIDE_GAME_END, HideGameEndPopUp);
        Messenger.AddListener(UIEvent.HIDE_ALL_POPUPS, HideAllPopUps);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(UIEvent.SHOW_HELP, ShowHelpPopUp);
        Messenger.RemoveListener(UIEvent.HIDE_HELP, HideHelpPopUp);
        Messenger.RemoveListener(UIEvent.SHOW_GAME_START, ShowGameStartPopUp);
        Messenger.RemoveListener(UIEvent.HIDE_GAME_START, HideGameStartPopUp);
        Messenger.RemoveListener(UIEvent.SHOW_GAME_END_WIN, ShowGameEndWinPopUp);
        Messenger.RemoveListener(UIEvent.SHOW_GAME_END_LOSE,ShowGameEndLosePopUp);
        Messenger.RemoveListener(UIEvent.HIDE_GAME_END, HideGameEndPopUp);
        Messenger.RemoveListener(UIEvent.HIDE_ALL_POPUPS, HideAllPopUps);
    }
}
