using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : MonoBehaviour
{
    public ParticleSystem explosion;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        explosion = Instantiate(explosion, transform.position, transform.rotation);
        explosion.Play();

        if (other.tag != "Ground")
        {
            Destroy(other.gameObject);
        }

        Destroy(gameObject);
    }
}
