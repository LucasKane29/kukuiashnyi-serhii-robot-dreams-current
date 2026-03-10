using System.Collections;
using System.Net;
using UnityEngine;
using Zenject;

public class BlasterWeapon : Weapon
{
    [SerializeField] private float range = 100f;

    [SerializeField] private LaserBolt laserBoltPrefab;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private float hitForce;
    [SerializeField] private GameObject flashEffect;
    private float flashLiveTime = 0.5f;

    public override void Fire()
    {
        if (!CanFire) 
            return;

        nextFireTime = Time.time + (1f / fireRate);
        SpawnLaserBolt();
        MadeShot();
    }

    private void SpawnLaserBolt()
    {
        if (laserBoltPrefab == null)
            return;

        LaserBolt bolt = Instantiate(laserBoltPrefab, muzzlePosition.position, muzzlePosition.rotation);
        bolt.Initialize(range, targetLayer, damage, hitForce);

        if (flashEffect != null)
        {
            GameObject flash = Instantiate(flashEffect, muzzlePosition.position, muzzlePosition.rotation);
            Destroy(flash, flashLiveTime);
        }
    }

    public override void Reload()
    {
        return;
    }
}
