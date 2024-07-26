using System;
using UnityEngine;

public class EMPGun : Gun
{
    protected override void Start()
    {
        base.Start();

        gunType = GunType.EMPGun;
    }

}