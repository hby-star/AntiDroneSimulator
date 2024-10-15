using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpBullet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            other.gameObject.GetComponent<Drone>().ReactToHit(Drone.HitType.ElectricInterference);
        }
    }
}