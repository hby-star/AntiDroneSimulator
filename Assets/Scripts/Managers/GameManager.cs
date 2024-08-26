using System;
using System.Collections;
using System.Collections.Generic;
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

    public enum GameState
    {
        MainMenu,
        Level1,
        Pause,
        GameOver,
    }

    public enum MissionState
    {
        Null,
        InMission,
        Success,
        Fail,
    }

    public GameState gameState { get; private set; }
    public MissionState missionState { get; private set; }

    private void Start()
    {
        gameState = GameState.MainMenu;
        missionState = MissionState.Null;

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // 启用Display 2
            Debug.Log("Display 2 activated.");
        }
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainMenu");
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

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level_1");
    }
}