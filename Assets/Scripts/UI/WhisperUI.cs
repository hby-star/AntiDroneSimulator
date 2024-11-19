using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Valve.VR;
using Whisper;
using Whisper.Utils;

public class WhisperUI : MonoBehaviour
{
    private WhisperManager whisper;
    private MicrophoneRecord microphoneRecord;
    private WhisperStream stream;
    private bool isRecording;

    public PlayerAgentManager playerAgentManager;
    public TextMeshProUGUI signalState;
    public TextMeshProUGUI robotMoveState;
    public TextMeshProUGUI robotAttackState;

    private void Awake()
    {
        whisper = SettingsManager.Instance.whisper.GetComponent<WhisperManager>();
        microphoneRecord = SettingsManager.Instance.whisper.GetComponent<MicrophoneRecord>();
        isRecording = false;
    }

    private async void Start()
    {
        stream = await whisper.CreateStream(microphoneRecord);
        microphoneRecord.StartRecord();
        stream.StopStream();
        signalState.text = "无命令";
        stream.OnStreamFinished += OnFinished;
    }

    private void OnFinished(string finalResult)
    {
        string text = "命令: ";
        if (finalResult.Contains("自动跟随"))
        {
            text += "自动跟随";
            robotMoveState.text = "自动跟随";
            playerAgentManager.SetRobotMoveState(PlayerAgentManager.RobotMoveState.Follow);
        }
        else if (finalResult.Contains("跟着我"))
        {
            text += "跟着我";
            robotMoveState.text = "自动跟随";
            playerAgentManager.SetRobotMoveState(PlayerAgentManager.RobotMoveState.Follow);
        }
        else if (finalResult.Contains("原地待命"))
        {
            text += "原地待命";
            robotMoveState.text = "原地待命";
            playerAgentManager.SetRobotMoveState(PlayerAgentManager.RobotMoveState.Stay);
        }
        else if (finalResult.Contains("返回基地"))
        {
            text += "返回基地";
            robotMoveState.text = "返回基地";
            playerAgentManager.SetRobotMoveState(PlayerAgentManager.RobotMoveState.Return);
        }
        else if (finalResult.Contains("自动攻击"))
        {
            text += "自动攻击";
            robotAttackState.text = "自动攻击";
            playerAgentManager.SetRobotAttackState(PlayerAgentManager.RobotAttackState.Attack);
        }
        else if (finalResult.Contains("停止攻击"))
        {
            text += "停止攻击";
            robotAttackState.text = "停止攻击";
            playerAgentManager.SetRobotAttackState(PlayerAgentManager.RobotAttackState.Stop);
        }
        else
        {
            text += "无法识别";
            playerAgentManager.SetRobotMoveState(PlayerAgentManager.RobotMoveState.Stay);
            playerAgentManager.SetRobotAttackState(PlayerAgentManager.RobotAttackState.Stop);
        }


        signalState.text = text;
        isRecording = false;
    }

    void HandleSignal()
    {
        if (!isRecording)
        {
            isRecording = true;
            stream.StartStream();
            signalState.text = "正在录音...";
        }
        else
        {
            stream.StopStream();
            signalState.text = "正在识别...";
        }
    }

    private void OnEnable()
    {
        Messenger.AddListener(InputEvent.PLAYER_SEND_SIGNAL_INPUT, HandleSignal);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(InputEvent.PLAYER_SEND_SIGNAL_INPUT, HandleSignal);
    }
}