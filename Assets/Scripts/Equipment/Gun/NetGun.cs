using System.Collections;
using UnityEngine;

public class NetGun : Gun
{
    public GameObject netPrefab; // Prefab for the net
    public float netSpeed = 10f;

    void SettingsAwake()
    {
        maxBullets = (int)SettingsManager.Instance.settings.GetComponent<Settings>().netBulletNumSlider.value;
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

        // Instantiate the net at the center of the screen
        GameObject net = Instantiate(netPrefab, firePosition.position, Quaternion.identity);

        // Apply an initial forward force to the net
        Rigidbody rb = net.GetComponent<Rigidbody>();
        net.transform.forward = firePosition.transform.forward;
        rb.velocity = firePosition.transform.forward * netSpeed;
    }
}