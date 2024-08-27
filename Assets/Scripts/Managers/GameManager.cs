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
            Debug.Log("Display 2 activated.");
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
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GameSuccess()
    {
        Messenger.Broadcast(UIEvent.SHOW_GAME_END_WIN);
        StopGame();
    }

    public void GameFail()
    {
        Messenger.Broadcast(UIEvent.SHOW_GAME_END_LOSE);
        StopGame();
    }

    IEnumerator LoadMainMenu()
    {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Messenger.Broadcast(UIEvent.HIDE_ALL_POPUPS);
    }

    IEnumerator LoadLevel1()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level_1");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        StopGame();
        Messenger.Broadcast(UIEvent.SHOW_GAME_START);
    }

    public void InGameMenu()
    {
        Messenger.Broadcast(UIEvent.SHOW_GAME_PAUSE);
        StopGame();
    }


    private void OnEnable()
    {
        Messenger.AddListener(GameEvent.START_GAME, StartGame);
        Messenger.AddListener(GameEvent.EXIT_GAME, QuitGame);
        Messenger.AddListener(GameEvent.GAME_SUCCESS, GameSuccess);
        Messenger.AddListener(GameEvent.GAME_FAIL, GameFail);
        Messenger.AddListener(GameEvent.STOP_GAME, StopGame);
        Messenger.AddListener(GameEvent.CONTINUE_GAME, ContinueGame);
        Messenger.AddListener(GameEvent.TO_MAIN_MENU, ToMainMenu);
        Messenger.AddListener(GameEvent.IN_GAME_MENU, InGameMenu);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.START_GAME, StartGame);
        Messenger.RemoveListener(GameEvent.EXIT_GAME, QuitGame);
        Messenger.RemoveListener(GameEvent.GAME_SUCCESS, GameSuccess);
        Messenger.RemoveListener(GameEvent.GAME_FAIL, GameFail);
        Messenger.RemoveListener(GameEvent.STOP_GAME, StopGame);
        Messenger.RemoveListener(GameEvent.CONTINUE_GAME, ContinueGame);
        Messenger.RemoveListener(GameEvent.TO_MAIN_MENU, ToMainMenu);
        Messenger.RemoveListener(GameEvent.IN_GAME_MENU, InGameMenu);
    }
}