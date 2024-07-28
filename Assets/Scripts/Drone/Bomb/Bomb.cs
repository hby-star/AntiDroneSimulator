using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : MonoBehaviour
{
    public ParticleSystem explosion;
    public float explosionRadius = 5f;
    private Drone drone;

    // Start is called before the first frame update
    void Start()
    {
        drone = GetComponentInParent<Drone>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(10, transform.position, explosionRadius);
            }

            if (nearbyObject.tag == "Player")
            {
                drone.hasBomb = false;
                explosion = Instantiate(explosion, transform.position, transform.rotation);
                explosion.Play();
                Destroy(nearbyObject.gameObject);
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}