using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunState : MonoBehaviour
{
    public Image gunImage;
    public TextMeshProUGUI bulletCount;

    public Player player;

    private void Start()
    {
        player.onGunChanged += UpdateGunUI;

        foreach (var gun in player.guns)
        {
            gun.onBulletCountChanged += UpdateGunBulletCount;
        }

        if (gunImage)
            gunImage.sprite = player.guns[0].gunImage;
        bulletCount.text = player.guns[0].currentBullets + " / " + player.guns[0].maxBullets;
    }

    void UpdateGunUI()
    {
        Gun currentGun = player.currentEquipment as Gun;
        if (currentGun)
        {
            if (gunImage)
                gunImage.sprite = currentGun.gunImage;
            bulletCount.text = currentGun.currentBullets + " / " + currentGun.maxBullets;
        }
    }

    void UpdateGunBulletCount()
    {
        Gun currentGun = player.currentEquipment as Gun;
        if (currentGun)
        {
            if (currentGun.isReloading)
            {
                bulletCount.text = "Reloading...";
            }
            else
            {
                bulletCount.text = currentGun.currentBullets + " / " + currentGun.maxBullets;
            }
        }
    }
}