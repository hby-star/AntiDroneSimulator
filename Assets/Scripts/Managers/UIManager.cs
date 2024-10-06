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
    public GameObject settingsPopUp;
    public GameObject gameStartPopUp;
    public GameObject gamePausePopUp;
    public GameObject gameEndPopUp;

    public void HideAllPopUps()
    {
        helpPopUp.SetActive(false);
        gameStartPopUp.SetActive(false);
        gameEndPopUp.SetActive(false);
        gamePausePopUp.SetActive(false);
        settingsPopUp.SetActive(false);
    }

    public bool IsPopUpAllHidden()
    {
        if (!helpPopUp.activeSelf && !gameStartPopUp.activeSelf && !gameEndPopUp.activeSelf &&
            !gamePausePopUp.activeSelf && !settingsPopUp.activeSelf)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowHelpPopUp()
    {
        helpPopUp.SetActive(true);
    }

    public void HideHelpPopUp()
    {
        helpPopUp.SetActive(false);
    }

    public void ShowSettings()
    {
        settingsPopUp.SetActive(true);
    }

    public void HideSettings()
    {
        settingsPopUp.SetActive(false);
    }

    public void ShowGameStartPopUp()
    {
        gameStartPopUp.SetActive(true);
    }

    public void HideGameStartPopUp()
    {
        gameStartPopUp.SetActive(false);
    }

    public void ShowGamePausePopUp()
    {
        gamePausePopUp.SetActive(true);
    }

    public void HideGamePausePopUp()
    {
        gamePausePopUp.SetActive(false);
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
                GameManager.Instance.ContinueGame();
            }
        }

        if (gameEndPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HideGameEndPopUp();
                GameManager.Instance.ToMainMenu();
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

        if (settingsPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        settingsPopUp.transform.GetChild(0).GetComponent<RectTransform>(), Input.mousePosition))
                {
                    HideSettings();
                }
            }
        }
    }
}