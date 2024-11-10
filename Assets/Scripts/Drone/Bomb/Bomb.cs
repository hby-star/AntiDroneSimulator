using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : MonoBehaviour
{
    public ParticleSystem explosion;
    public float explosionRadius = 10f;
    public AudioClip explosionSound;
    public float damage = 30f;
    public bool canExplode = false;

    private void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            explosionRadius = SettingsManager.Instance.settings.GetComponent<Settings>().bombRangeSlider.value;
            damage = SettingsManager.Instance.settings.GetComponent<Settings>().bombDamageSlider.value;
        }
    }

    void Awake()
    {
        SettingsAwake();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canExplode || other.CompareTag("Drone"))
        {
            return;
        }

        ParticleSystem explosionEffect = Instantiate(explosion, transform.position, transform.rotation);
        explosionEffect.Play();
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);

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