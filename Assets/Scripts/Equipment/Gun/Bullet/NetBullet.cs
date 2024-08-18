using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetBullet : MonoBehaviour
{
    public GameObject spherePrefab; // Prefab for the sphere

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Drone"))
        {
            // Transform the net into a sphere
            Vector3 collisionPoint = collision.contacts[0].point;
            collision.gameObject.GetComponent<OperableDrone>().ReactToHit(OperableDrone.HitType.NetBullet);
            Destroy(gameObject);

            GameObject sphere = Instantiate(spherePrefab, collisionPoint, Quaternion.identity);
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero; // Stop the sphere's movement

            // Destroy the sphere after 2 seconds
            Destroy(sphere, 2f);
        }
    }
}
