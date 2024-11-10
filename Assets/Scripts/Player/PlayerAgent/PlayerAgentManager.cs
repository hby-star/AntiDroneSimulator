using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgentManager : MonoBehaviour
{
    PlayerAgent[] playerAgents;

    private void Awake()
    {
        playerAgents = GetComponentsInChildren<PlayerAgent>();
        int playerAgentNum = (int)SettingsManager.Instance.settings.GetComponent<Settings>().agentNumSlider.value;
        for (int i = 0; i < playerAgentNum; i++)
        {
            playerAgents[i].gameObject.SetActive(true);
        }
        for (int i = playerAgentNum; i < playerAgents.Length; i++)
        {
            playerAgents[i].gameObject.SetActive(false);
        }
    }
}