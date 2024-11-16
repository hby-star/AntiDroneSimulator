using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Whisper;
using Whisper.Utils;

public class WhisperUI : MonoBehaviour
{
    private WhisperManager whisper;
    private MicrophoneRecord microphoneRecord;
    private WhisperStream stream;

    public SteamVR_Action_Boolean whisperAction;

    private void Awake()
    {
        whisper = SettingsManager.Instance.whisper.GetComponent<WhisperManager>();
        microphoneRecord = SettingsManager.Instance.whisper.GetComponent<MicrophoneRecord>();
    }

    private async void Start()
    {
        stream = await whisper.CreateStream(microphoneRecord);
        stream.OnStreamFinished += OnFinished;
    }

    private void OnFinished(string finalResult)
    {
        print("Stream finished: " + finalResult);
    }

    private void Update()
    {

    }
}
