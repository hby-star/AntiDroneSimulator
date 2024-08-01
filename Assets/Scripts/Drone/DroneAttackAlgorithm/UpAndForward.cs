using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpAndForward : IDroneAttackAlgorithm
{
    private Drone currentDrone;

    public void DroneAttackAlgorithmSet(Drone drone)
    {
        currentDrone = drone;
    }

    public void DroneAttackAlgorithmUpdate()
    {
        // 先向上飞一段距离，再向玩家飞去
        if (Mathf.Abs(currentDrone.transform.position.y - currentDrone.TargetPlayer.transform.position.y) < 50f)
        {
            Vector3 upDirection = new Vector3(0, 1, 0);
            currentDrone.transform.position += upDirection * currentDrone.moveSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 forwardDirection = (currentDrone.TargetPlayer.transform.position - currentDrone.transform.position).normalized;
            currentDrone.transform.position += forwardDirection * currentDrone.moveSpeed * Time.deltaTime;
        }
    }
}
