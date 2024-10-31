using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

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

    #region PopUps

    public GameObject helpPopUp;
    public GameObject settingsPopUp;
    public GameObject gameStartPopUp;
    public GameObject gamePausePopUp;
    public GameObject gameEndPopUp;

    public void HideAllPopUps()
    {
        if (helpPopUp)
        {
            helpPopUp.SetActive(false);
        }
        if (gameStartPopUp)
        {
            gameStartPopUp.SetActive(false);
        }
        if (gameEndPopUp)
        {
            gameEndPopUp.SetActive(false);
        }
        if (gamePausePopUp)
        {
            gamePausePopUp.SetActive(false);
        }
        if (settingsPopUp)
        {
            settingsPopUp.SetActive(false);
        }
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
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Win!";
        gameEndPopUp.SetActive(true);
        Cursor.visible = true;
    }

    public void ShowGameEndLosePopUp()
    {
        gameEndPopUp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You Lose!";
        gameEndPopUp.SetActive(true);
        Cursor.visible = true;
    }

    public void HideGameEndPopUp()
    {
        gameEndPopUp.SetActive(false);
    }

    #endregion

    #region XR input

    InputDevice _leftController;
    InputDevice _rightController;

    void XRDeviceStart()
    {
        _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    bool IsLeftTriggerPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.triggerButton, out value);
        return value;
    }

    bool IsRightTriggerPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.triggerButton, out value);
        return value;
    }

    bool IsLeftGripPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.gripButton, out value);
        return value;
    }

    bool IsRightGripPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.gripButton, out value);
        return value;
    }

    bool IsLeftPrimaryButtonPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.primaryButton, out value);
        return value;
    }

    bool IsRightPrimaryButtonPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.primaryButton, out value);
        return value;
    }

    bool IsLeftSecondaryButtonPressed()
    {
        bool value;
        _leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out value);
        return value;
    }

    bool IsRightSecondaryButtonPressed()
    {
        bool value;
        _rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out value);
        return value;
    }

    #endregion


    void Start()
    {
        HideAllPopUps();
        XRDeviceStart();
    }


    void Update()
    {
        if (gameStartPopUp&&gameStartPopUp.activeSelf)
        {
            if (IsLeftTriggerPressed() || IsRightTriggerPressed() || Input.GetMouseButtonDown(0))
            {
                HideGameStartPopUp();
                GameManager.Instance.ContinueGame();
            }
        }

        if (gamePausePopUp&&gamePausePopUp.activeSelf)
        {
            if (IsLeftTriggerPressed() || IsRightTriggerPressed() || Input.GetMouseButtonDown(0))
            {
                HideGamePausePopUp();
                GameManager.Instance.ContinueGame();
            }
        }

        if (gameEndPopUp&&gameEndPopUp.activeSelf || Input.GetMouseButtonDown(0))
        {
            if (IsLeftTriggerPressed() || IsRightTriggerPressed())
            {
                HideGameEndPopUp();
                GameManager.Instance.ToMainMenu();
            }
        }

        if (helpPopUp&&helpPopUp.activeSelf)
        {
            if (IsLeftGripPressed() || IsRightGripPressed() || Input.GetKeyDown(KeyCode.Escape))
            {
                HideHelpPopUp();
            }
        }

        if (settingsPopUp&&settingsPopUp.activeSelf)
        {
            if (IsLeftGripPressed() || IsRightGripPressed() || Input.GetKeyDown(KeyCode.Escape))
            {
                HideSettings();
            }
        }
    }
}