using System;
using UnityEngine;

public class EMPGun : Gun
{
    public float bulletSize = 50f;

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
                        drone.ReactToHit(Drone.HitType.EmpBullet);
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (InputManager.Instance.operateEntityNow && InputManager.Instance.currentEntity is Player player && player.currentEquipment == this)
        {
            int lineLength = 6;
            int lineWidth = 2;
            int circleRadius = 30;
            int circleSegments = 100;
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;

            // Create a 1x1 texture for lines
            Texture2D lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();

            // Horizontal line
            GUI.DrawTexture(new Rect(centerX - lineLength / 2, centerY - lineWidth / 2, lineLength, lineWidth), lineTexture);
            // Vertical line
            GUI.DrawTexture(new Rect(centerX - lineWidth / 2, centerY - lineLength / 2, lineWidth, lineLength), lineTexture);

            // Draw circular outline
            for (int i = 0; i < circleSegments; i++)
            {
                float angle = i * Mathf.PI * 2 / circleSegments;
                float x = Mathf.Cos(angle) * circleRadius;
                float y = Mathf.Sin(angle) * circleRadius;

                GUI.DrawTexture(new Rect(centerX + x - lineWidth / 2, centerY + y - lineWidth / 2, lineWidth, lineWidth), lineTexture);
            }
        }
    }
}