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

    public GameObject gameStartPopUp;
    public GameObject gamePausePopUp;
    public GameObject gameEndPopUp;

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
    }

    public void HideGameStartPopUp()
    {
        gameStartPopUp.SetActive(false);
        Cursor.visible = false;
    }

    public void ShowGamePausePopUp()
    {
        gamePausePopUp.SetActive(true);
        Cursor.visible = true;
    }

    public void HideGamePausePopUp()
    {
        gamePausePopUp.SetActive(false);
        Cursor.visible = false;
    }

    public void ShowGameEndWinPopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "任务成功!";
        gameEndPopUp.SetActive(true);
        Cursor.visible = true;
    }

    public void ShowGameEndLosePopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "任务失败!";
        gameEndPopUp.SetActive(true);
        Cursor.visible = true;
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

    }
}