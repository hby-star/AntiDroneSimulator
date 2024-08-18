using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownAndForward : IDroneAttackAlgorithm
{
    private OperableDrone currentOperableDrone;

    public void DroneAttackAlgorithmSet(OperableDrone operableDrone)
    {
        currentOperableDrone = operableDrone;
    }

    public void DroneAttackAlgorithmUpdate()
    {
        // // 先向下飞一段距离，再向玩家飞去
        // if (Mathf.Abs(currentDrone.transform.position.y - currentDrone.TargetPlayer.transform.position.y) > 10f)
        // {
        //     Vector3 downDirection = new Vector3(0, -1, 0);
        //     currentDrone.transform.position += downDirection * currentDrone.moveSpeed * Time.deltaTime;
        // }
        // else
        // {
        //     Vector3 forwardDirection = (currentDrone.TargetPlayer.transform.position - currentDrone.transform.position).normalized;
        //     currentDrone.transform.position += forwardDirection * currentDrone.moveSpeed * Time.deltaTime;
        // }
    }
}
