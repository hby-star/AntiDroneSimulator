using UnityEngine;

public class NetGun : Gun
{
    protected override void Start()
    {
        base.Start();

        gunType = GunType.NetGun;
    }

    public override void Fire()
    {
        currentBullets--;
        Debug.Log("Fire NetGun");
    }
}