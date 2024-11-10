using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    public bool IsGamePaused { get; private set; }

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

    private void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // 启用Display 2
        }
    }

    public void StartGame()
    {
        StartCoroutine(LoadLevel1());
    }

    public void ToMainMenu()
    {
        StartCoroutine(LoadMainMenu());
    }

    public void StopGame()
    {
        StopGameInput();
        Time.timeScale = 0;
        IsGamePaused = true;
    }

    public void StopGameInput()
    {
        InputManager.Instance.gameObject.SetActive(false);
        GameObject.FindWithTag("Player").GetComponent<Player>().ZeroInput();
    }

    public void ContinueGameInput()
    {
        InputManager.Instance.gameObject.SetActive(true);
    }

    public void ContinueGame()
    {
        ContinueGameInput();
        Time.timeScale = 1;
        IsGamePaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GameSuccess()
    {
        UIManager.Instance.ShowGameEndWinPopUp();
        StopGame();
    }

    public void GameFail()
    {
        UIManager.Instance.ShowGameEndLosePopUp();
        StopGame();
    }

    IEnumerator LoadMainMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //UIManager.Instance.HideAllPopUps();
        SettingsManager.Instance.settings.SetActive(true);
    }

    IEnumerator LoadLevel1()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level_1");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //StopGame();
        //UIManager.Instance.ShowGameStartPopUp();
        SettingsManager.Instance.settings.SetActive(false);
    }

    public void InGameMenu()
    {
        UIManager.Instance.ShowGamePausePopUp();
        StopGame();
    }
}