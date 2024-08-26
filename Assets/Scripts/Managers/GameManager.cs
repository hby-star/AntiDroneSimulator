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

    public GameObject gameEndPopUp;
    private void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // 启用Display 2
            Debug.Log("Display 2 activated.");
        }

        gameEndPopUp.SetActive(false);
        DontDestroyOnLoad(gameEndPopUp);
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartGame()
    {
        LoadLevel1();
    }

    public void StopGame()
    {
        Time.timeScale = 0;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GameSuccess()
    {
        gameEndPopUp.SetActive(true);
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Win!";
    }

    public void GameFail()
    {
        gameEndPopUp.SetActive(true);
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Lose!";
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level_1");
    }


    private void OnEnable()
    {
        Messenger.AddListener(GameEvent.START_GAME, StartGame);
        Messenger.AddListener(GameEvent.EXIT_GAME, QuitGame);
        Messenger.AddListener(GameEvent.GAME_SUCCESS, GameSuccess);
        Messenger.AddListener(GameEvent.GAME_FAIL, GameFail);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.START_GAME, StartGame);
        Messenger.RemoveListener(GameEvent.EXIT_GAME, QuitGame);
        Messenger.RemoveListener(GameEvent.GAME_SUCCESS, GameSuccess);
        Messenger.RemoveListener(GameEvent.GAME_FAIL, GameFail);
    }

    void Update()
    {
        if (gameEndPopUp.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gameEndPopUp.SetActive(false);
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}