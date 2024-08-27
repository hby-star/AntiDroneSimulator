using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : MonoBehaviour
{
    public ParticleSystem explosion;
    public float explosionRadius = 10f;

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
        if (other.tag == "Drone")
        {
            return;
        }

        explosion = Instantiate(explosion, transform.position, transform.rotation);
        explosion.Play();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Vector3 directionToTarget = nearbyObject.transform.position - transform.position;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToTarget, explosionRadius);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == nearbyObject && nearbyObject.tag == "Player")
                {
                    //Debug.Log("Player hit by bomb");
                    Messenger.Broadcast(GameEvent.GAME_FAIL);
                    break;
                }
                else if (hit.collider != nearbyObject)
                {
                    // If there's an obstruction, break out of the loop
                    break;
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}