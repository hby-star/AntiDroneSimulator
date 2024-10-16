using System;
using UnityEngine;

public class EMPGun : Gun
{
    public GameObject empBulletPrefab;
    public float empBulletSpeed = 10f;

    void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            maxBullets = (int)UIManager.Instance.settingsPopUp.GetComponent<Settings>().empBulletNumSlider.value;
            currentBullets = maxBullets;
        }
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
        Vector3 origin = playerCamera.transform.position + playerCamera.transform.forward * 2;
        GameObject bullet = Instantiate(empBulletPrefab, origin, Quaternion.identity);

        // Apply an initial forward force to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.forward = playerCamera.transform.forward;
        rb.velocity = playerCamera.transform.forward * empBulletSpeed;
        rb.useGravity = false;
    }
}