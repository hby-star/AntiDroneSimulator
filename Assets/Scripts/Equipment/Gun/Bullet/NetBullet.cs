using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetBullet : MonoBehaviour
{
    public GameObject spherePrefab; // Prefab for the sphere

    private float lifeTime = 10f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Drone"))
        {
            // Transform the net into a sphere
            Vector3 collisionPoint = collision.contacts[0].point;
            collision.gameObject.GetComponent<Drone>().ReactToHit(Drone.HitType.NetBullet);
            Destroy(gameObject);

            GameObject sphere = Instantiate(spherePrefab, collisionPoint, Quaternion.identity);
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero; // Stop the sphere's movement

            // Destroy the sphere after 1 seconds
            Destroy(sphere, 1f);
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}