using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forward : IDroneAttackAlgorithm
{
    private Drone currentDrone;

    public void DroneAttackAlgorithmSet(Drone drone)
    {
        currentDrone = drone;
    }

    public void DroneAttackAlgorithmUpdate()
    {
        // 直接向玩家飞去
        Vector3 direction = currentDrone.TargetPlayer.transform.position - currentDrone.transform.position;
        currentDrone.transform.position += direction.normalized * currentDrone.moveSpeed * Time.deltaTime;
    }
}
