using System;
using UnityEngine;

public class EMPGun : Gun
{
    public GameObject empBulletPrefab;
    public float empBulletSpeed = 10f;

    void SettingsAwake()
    {
        maxBullets = (int)SettingsManager.Instance.settings.GetComponent<Settings>().empBulletNumSlider.value;
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
        GameObject bullet = Instantiate(empBulletPrefab, firePosition.position, Quaternion.identity);

        // Apply an initial forward force to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.forward = firePosition.transform.forward;
        rb.velocity = firePosition.transform.forward * empBulletSpeed;
        rb.useGravity = false;
    }
}