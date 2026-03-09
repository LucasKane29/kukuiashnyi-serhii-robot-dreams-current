using System.Collections;
using System.Net;
using UnityEngine;

public class BlasterWeapon : Weapon
{
    [SerializeField] private float range = 100f;

    [SerializeField] private LaserBolt laserBoltPrefab;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private float hitForce;

    public override void Fire()
    {
        if (!CanFire) 
            return;

        nextFireTime = Time.time + (1f / fireRate);
        SpawnLaserBolt();
    }

    private void SpawnLaserBolt()
    {
        if (laserBoltPrefab == null)
            return;

        LaserBolt bolt = Instantiate(laserBoltPrefab, muzzlePosition.position, muzzlePosition.rotation);
        bolt.Initialize(range, targetLayer, damage, hitForce);
    }

    public override void Reload()
    {
        return;
    }
}
