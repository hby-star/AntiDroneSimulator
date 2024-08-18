using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDroneAttackAlgorithm
{
    void DroneAttackAlgorithmSet(OperableDrone operableDrone);
    void DroneAttackAlgorithmUpdate();
}
