using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : MonoBehaviour
{
    public ParticleSystem explosion;
    public float explosionRadius = 10f;
    public float damage = 30f;
    public bool canExplode = false;

    private void SettingsStart()
    {
        if (UIManager.Instance)
        {
            explosionRadius = UIManager.Instance.settingsPopUp.GetComponent<Settings>().bombRangeSlider.value;
            damage = UIManager.Instance.settingsPopUp.GetComponent<Settings>().bombDamageSlider.value;
        }
    }

    void Start()

    {
        SettingsStart();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!canExplode)
        {
            return;
        }

        explosion = Instantiate(explosion, transform.position, transform.rotation);
        explosion.Play();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Player"))
            {
                nearbyObject.GetComponent<Player>().TakeDamage(damage);
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