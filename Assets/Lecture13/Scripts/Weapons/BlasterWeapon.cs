using System.Collections;
using System.Net;
using UnityEngine;

public class BlasterWeapon : Weapon
{
    [SerializeField] private float range = 100f;

    [SerializeField] private LaserBolt laserBoltPrefab;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private float hitForce;
    [SerializeField] private GameObject flashEffect;
    private float flashLiveTime = 0.5f;

    public override void Fire(Vector3? targetDirection = null)
    {
        if (!CanFire) 
            return;

        nextFireTime = Time.time + (1f / fireRate);
        SpawnLaserBolt(targetDirection);
        MadeShot();
        PlayFireSound();
    }

    private void SpawnLaserBolt(Vector3? targetDirection = null)
    {
        if (laserBoltPrefab == null)
            return;

        Quaternion spawnRotation;
        if (targetDirection != null)
        {
            Vector3 dir = (targetDirection.Value - muzzlePosition.position).normalized;
            spawnRotation = Quaternion.LookRotation(dir);
        }
        else
        {
            spawnRotation = muzzlePosition.rotation;
        }

        LaserBolt bolt = Instantiate(laserBoltPrefab, muzzlePosition.position, spawnRotation);
        bolt.Initialize(range, targetLayer, damage, hitForce, muzzlePosition.position, targetDirection);

        if (flashEffect != null)
        {
            if (_isPlayerWeapon)
            {
                GameObject flash = Instantiate(flashEffect, muzzlePosition.position, muzzlePosition.rotation, muzzlePosition);
                Destroy(flash, flashLiveTime);
            }
            else
            {
                GameObject flash = Instantiate(flashEffect, muzzlePosition.position, muzzlePosition.rotation);
                Destroy(flash, flashLiveTime);
            }
        }
    }

    public override void Reload()
    {
        return;
    }

    public Vector3 GetMuzzlePosition()
    {
        return muzzlePosition.position;
    }
}
