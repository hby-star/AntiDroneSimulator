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

    private void SettingsStart()
    {
        explosionRadius = UIManager.Instance.settingsPopUp.GetComponent<Settings>().bombRangeSlider.value;
        damage = UIManager.Instance.settingsPopUp.GetComponent<Settings>().bombDamageSlider.value;
    }

    void Start()

    {
        SettingsStart();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drone"))
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
                // 检测Player与炸弹间是否有障碍物
                RaycastHit hit;
                if (Physics.Linecast(transform.position, nearbyObject.transform.position, out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        nearbyObject.GetComponent<Player>().TakeDamage(damage);
                    }
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