using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class HandGun : Gun
{
    public GameObject bulletImpact;
    public Transform fireEffectPosition;
    public ParticleSystem fireEffectPrefab;

    void SettingsAwake()
    {
        if (UIManager.Instance)
        {
            maxBullets = (int)SettingsManager.Instance.settings.GetComponent<Settings>().normalBulletNumSlider.value;
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

        // Play the fire effect
        ParticleSystem fireEffect = Instantiate(fireEffectPrefab, fireEffectPosition.position, fireEffectPosition.rotation);
        fireEffect.transform.parent = fireEffectPosition;
        fireEffect.Play();

        Vector3 point = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = playerCamera.ScreenPointToRay(point);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            Drone drone = hitObject.GetComponent<Drone>();
            if (drone)
            {
                drone.ReactToHit(Drone.HitType.NormalBullet);
            }
            else
            {
                if (hitObject.CompareTag("Ground"))
                {
                    StartCoroutine(AttackBulletImpact(hit.point, hit.normal));
                }
            }
        }
    }



    private IEnumerator AttackBulletImpact(Vector3 pos, Vector3 normal)
    {
        // Calculate the rotation so that the prefab's Y-axis points in the direction of the normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Instantiate the bullet impact prefab with the adjusted rotation
        GameObject impactEffect = Instantiate(bulletImpact, pos, rotation);

        // Optional: Adjust if the effect should disappear after some time
        yield return new WaitForSeconds(1);

        Destroy(impactEffect);
    }
}