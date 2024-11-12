using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    private float lifeTime = 5f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            other.gameObject.GetComponent<Drone>().ReactToHit(Drone.HitType.NormalBullet);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}