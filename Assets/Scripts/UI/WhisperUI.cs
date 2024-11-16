using System;
using System.Collections;
using System.Collections.Generic;
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
        signalState.text = "命令: " + finalResult;
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