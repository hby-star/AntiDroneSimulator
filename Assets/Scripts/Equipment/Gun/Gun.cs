using UnityEngine;

public class Gun : Equipment
{
    public Camera playerCamera;

    [Header("Audio Info")] [SerializeField]
    public AudioSource soundSource;

    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip fireSound;


    public enum GunType
    {
        HandGun,
        NetGun,
        EMPGun,
    }

    public int currentBullets;
    public int maxBullets;
    public GunType gunType;

    protected virtual void Start()
    {
        Type = EquipmentType.Gun;
    }

    public virtual void Fire()
    {
        currentBullets--;
        soundSource.PlayOneShot(fireSound);
    }

    public virtual bool CanFire()
    {
        return currentBullets > 0;
    }

    public virtual void Reload()
    {
        currentBullets = maxBullets;
        soundSource.PlayOneShot(reloadSound);
    }

    public virtual bool CanReload()
    {
        return currentBullets < maxBullets;
    }
}