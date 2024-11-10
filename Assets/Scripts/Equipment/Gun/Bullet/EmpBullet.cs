using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpBullet : MonoBehaviour
{
    public AudioSource audioSource;

    private float lifeTime = 10f;
    private float timer = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = SettingsManager.Instance.settings.GetComponent<Settings>().volumeSlider.value / 2f;
        audioSource.Play();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            other.gameObject.GetComponent<Drone>().ReactToHit(Drone.HitType.ElectricInterference);
        }
    }
}