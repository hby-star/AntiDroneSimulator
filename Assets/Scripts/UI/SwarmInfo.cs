using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwarmInfo : MonoBehaviour
{
    public Swarm swarm;
    public TextMeshProUGUI droneCountText;

    private void Start()
    {
        swarm.OnDroneCountChanged += UpdateDroneCount;
        droneCountText.text = swarm.droneCount.ToString();
    }

    private void UpdateDroneCount()
    {
        droneCountText.text = swarm.droneCount.ToString();
    }
}
