using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgentManager : MonoBehaviour
{
    PlayerAgent[] playerAgents;

    public enum RobotMoveState
    {
        Follow,
        Stay,
        Return
    }

    public enum RobotAttackState
    {
        Attack,
        Stop
    }

    public RobotMoveState robotMoveState;
    public RobotAttackState robotAttackState;

    private void Awake()
    {
        playerAgents = GetComponentsInChildren<PlayerAgent>();
        int playerAgentNum = (int)SettingsManager.Instance.settings.GetComponent<Settings>().agentNumSlider.value;

        robotMoveState = RobotMoveState.Stay;
        robotAttackState = RobotAttackState.Stop;
        for (int i = playerAgentNum; i < playerAgents.Length; i++)
        {
            playerAgents[i].robotId = i;
            playerAgents[i].moveState = robotMoveState;
            playerAgents[i].attackState = robotAttackState;

            if (i < playerAgentNum)
            {
                playerAgents[i].gameObject.SetActive(true);
            }
            else
            {
                playerAgents[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetRobotMoveState(RobotMoveState state)
    {
        robotMoveState = state;
        foreach (PlayerAgent playerAgent in playerAgents)
        {
            playerAgent.moveState = robotMoveState;
        }
    }

    public void SetRobotAttackState(RobotAttackState state)
    {
        robotAttackState = state;
        foreach (PlayerAgent playerAgent in playerAgents)
        {
            playerAgent.attackState = robotAttackState;
        }
    }
}