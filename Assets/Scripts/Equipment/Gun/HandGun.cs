using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class HandGun : Gun
{
    public GameObject normalBulletPrefab;
    public float normalBulletSpeed = 1000f;

    void SettingsAwake()
    {
        maxBullets = (int)SettingsManager.Instance.settings.GetComponent<Settings>().normalBulletNumSlider.value;
        currentBullets = maxBullets;
    }

    protected override void Awake()
    {
        base.Awake();

        SettingsAwake();
    }


    public override void Fire()
    {
        base.Fire();

        // Instantiate the bullet at the center of the screen
        GameObject bullet = Instantiate(normalBulletPrefab, firePosition.position, Quaternion.identity);

        // Apply an initial forward force to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.forward = firePosition.transform.forward;
        rb.velocity = firePosition.transform.forward * normalBulletSpeed;
    }
}