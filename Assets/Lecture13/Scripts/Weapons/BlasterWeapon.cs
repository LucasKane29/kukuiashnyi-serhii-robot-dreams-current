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
    private Quaternion _originalMuzzleRotation;

    public override void Fire(Vector3? targetDirection = null)
    {
        if (!CanFire) 
            return;

        nextFireTime = Time.time + (1f / fireRate);
        SpawnLaserBolt(targetDirection);
        MadeShot();
    }

    private void SpawnLaserBolt(Vector3? targetDirection = null)
    {
        if (laserBoltPrefab == null)
            return;

        if(targetDirection != null)
        {
            _originalMuzzleRotation = Quaternion.LookRotation(targetDirection.Value - muzzlePosition.position).normalized;
        }
        else
        {
            _originalMuzzleRotation = muzzlePosition.rotation;
        }

        LaserBolt bolt = Instantiate(laserBoltPrefab, muzzlePosition.position, muzzlePosition.rotation);
        bolt.Initialize(range, targetLayer, damage, hitForce, muzzlePosition.position, targetDirection);

        if (flashEffect != null)
        {
            GameObject flash = Instantiate(flashEffect, muzzlePosition.position, muzzlePosition.rotation, muzzlePosition);
            Destroy(flash, flashLiveTime);
        }
    }

    public override void Reload()
    {
        return;
    }
}
