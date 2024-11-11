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
        Vector3 origin = playerCamera.transform.position + playerCamera.transform.forward;
        GameObject net = Instantiate(netPrefab, origin, Quaternion.identity);

        // Apply an initial forward force to the net
        Rigidbody rb = net.GetComponent<Rigidbody>();
        net.transform.forward = playerCamera.transform.forward;
        rb.velocity = playerCamera.transform.forward * netSpeed;
    }
}