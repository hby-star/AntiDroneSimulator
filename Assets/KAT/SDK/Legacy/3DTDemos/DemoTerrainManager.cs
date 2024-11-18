using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTerrainManager : MonoBehaviour
{
    public GameObject player;
    public GameObject playerRotation;

    void Awake()
    {
        _3DTerrainPlatform.Create(player, playerRotation.transform);
    }

  
}
