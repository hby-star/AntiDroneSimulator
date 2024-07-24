using System.Collections.Generic;
using UnityEngine;


public interface IDroneControlAlgorithm
{
    void DroneControlSet(Drone drone);
    void DroneControlUpdate();

}
