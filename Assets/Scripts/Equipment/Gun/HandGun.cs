﻿using System.Collections;
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
                drone.ReactToHit();
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

    void OnGUI()
    {
        if (InputManager.Instance.operateEntityNow)
        {
            int crosshairSize = 12;
            int lineLength = 6;
            int lineWidth = 2;
            float posX = Screen.width / 2 - crosshairSize / 4;
            float posY = Screen.height / 2 - crosshairSize / 2;

            // Create a 1x1 texture for lines
            Texture2D lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();

            // Horizontal line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 - lineLength / 2, Screen.height / 2 - lineWidth / 2,
                    lineLength,
                    lineWidth), lineTexture);
            // Vertical line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 - lineWidth / 2, Screen.height / 2 - lineLength / 2,
                    lineWidth,
                    lineLength), lineTexture);
            // Left line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 - lineLength - crosshairSize / 2,
                    Screen.height / 2 - lineWidth / 2,
                    lineLength, lineWidth), lineTexture);
            // Right line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 + crosshairSize / 2, Screen.height / 2 - lineWidth / 2,
                    lineLength,
                    lineWidth), lineTexture);
            // Top line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 - lineWidth / 2,
                    Screen.height / 2 - lineLength - crosshairSize / 2,
                    lineWidth, lineLength), lineTexture);
            // Bottom line
            GUI.DrawTexture(
                new Rect(Screen.width / 2 - lineWidth / 2, Screen.height / 2 + crosshairSize / 2,
                    lineWidth,
                    lineLength), lineTexture);
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