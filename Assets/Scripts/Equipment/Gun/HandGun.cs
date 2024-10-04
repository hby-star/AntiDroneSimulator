using System.Collections;
using UnityEngine;

public class HandGun : Gun
{
    public GameObject bulletImpact;

    protected override void Start()
    {
        base.Start();

        gunType = GunType.HandGun;
    }

    public override void Fire()
    {
        base.Fire();

        Vector3 point = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = playerCamera.ScreenPointToRay(point);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            Drone drone = hitObject.GetComponent<Drone>();
            if (drone != null)
            {
                drone.ReactToHit(Drone.HitType.NormalBullet);
            }
            else
            {
                if (hitObject.tag == "Ground")
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