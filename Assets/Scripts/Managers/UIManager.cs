using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using Cursor = UnityEngine.Cursor;

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
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Game Start
    [Header("Game Start")]
    public GameObject gameStartPopUp;
    public Button gameStartContinueButton;

    #endregion

    #region Game Pause
    [Header("Game Pause")]
    public GameObject gamePausePopUp;
    public Button gamePauseContinueButton;
    public Button gamePauseReturnMainMenuButton;

    #endregion

    #region Game End
    [Header("Game End")]
    public GameObject gameEndPopUp;
    public Button gameEndContinueButton;

    #endregion

    public void HideAllPopUps()
    {
        gameStartPopUp.SetActive(false);
        gameEndPopUp.SetActive(false);
        gamePausePopUp.SetActive(false);
    }

    public bool IsPopUpAllHidden()
    {
        if (!gameStartPopUp.activeSelf && !gameEndPopUp.activeSelf &&
            !gamePausePopUp.activeSelf)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowGameStartPopUp()
    {
        gameStartPopUp.SetActive(true);
        GameManager.Instance.StopGameInput();
    }

    public void HideGameStartPopUp()
    {
        gameStartPopUp.SetActive(false);
        GameManager.Instance.ContinueGameInput();
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
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "任务成功!";
        gameEndPopUp.SetActive(true);
    }

    public void ShowGameEndLosePopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "任务失败!";
        gameEndPopUp.SetActive(true);
    }

    public void HideGameEndPopUp()
    {
        gameEndPopUp.SetActive(false);
    }

    void Start()
    {
        gameStartContinueButton.onClick.AddListener(OnGameStartContinueButtonClick);

        gamePauseContinueButton.onClick.AddListener(OnGamePauseContinueButtonClick);
        gamePauseReturnMainMenuButton.onClick.AddListener(OnGamePauseReturnMainMenuButtonClick);

        gameEndContinueButton.onClick.AddListener(OnGameEndContinueButtonClick);

        // UIElement gameEndPopUpUIElement = gameEndPopUp.GetComponent<UIElement>();
        // gameEndPopUpUIElement.onHandClick.AddListener(HideGameEndPopUp);
    }

    void OnGameStartContinueButtonClick()
    {
        HideGameStartPopUp();
        GameManager.Instance.ContinueGame();
    }

    void OnGamePauseContinueButtonClick()
    {
        HideGamePausePopUp();
        GameManager.Instance.ContinueGame();
    }

    void OnGamePauseReturnMainMenuButtonClick()
    {
        HideGamePausePopUp();
        GameManager.Instance.ToMainMenu();
    }

    void OnGameEndContinueButtonClick()
    {
        HideGameEndPopUp();
        GameManager.Instance.ToMainMenu();
    }

    // void Update()
    // {
    //     if (gameStartPopUp.activeSelf)
    //     {
    //         if (Input.GetMouseButtonDown(0))
    //         {
    //             HideGameStartPopUp();
    //             GameManager.Instance.ContinueGame();
    //         }
    //     }
    //
    //     if (gameEndPopUp.activeSelf)
    //     {
    //         if (Input.GetMouseButtonDown(0))
    //         {
    //             HideGameEndPopUp();
    //             GameManager.Instance.ToMainMenu();
    //         }
    //     }
    //
    // }
}