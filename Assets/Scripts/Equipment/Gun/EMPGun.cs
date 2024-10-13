using System;
using UnityEngine;

public class EMPGun : Gun
{
    protected override void Start()
    {
        base.Start();

        gunType = GunType.EMPGun;
    }

    public override void Fire()
    {
        base.Fire();

        float laserRange = 100f;
        float laserAngle = 20f;
        int numberOfRays = 20;

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        for (int i = 0; i < numberOfRays; i++)
        {
            for (int j = 0; j < numberOfRays; j++)
            {
                float horizontalAngle = Mathf.Lerp(-laserAngle / 2, laserAngle / 2, (float)i / (numberOfRays - 1));
                float verticalAngle = Mathf.Lerp(-laserAngle / 2, laserAngle / 2, (float)j / (numberOfRays - 1));
                Vector3 spreadDirection = Quaternion.Euler(verticalAngle, horizontalAngle, 0) * direction;

                if (Physics.Raycast(origin, spreadDirection, out RaycastHit hit, laserRange))
                {
                    Drone drone = hit.collider.GetComponent<Drone>();
                    if (drone != null)
                    {
                        drone.ReactToHit(Drone.HitType.ElectricInterference);
                    }
                }
            }
        }
    }
}