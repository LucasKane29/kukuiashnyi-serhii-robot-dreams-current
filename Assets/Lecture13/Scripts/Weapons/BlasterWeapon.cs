using System.Collections;
using System.Net;
using UnityEngine;

public class BlasterWeapon : Weapon
{
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private GameObject laserBoltPrefab;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private float hitForce;

    public override void Fire(Transform firePoint)
    {
        if (!CanFire) 
            return;

        nextFireTime = Time.time + (1f / fireRate);
        Vector3 targetPoint = firePoint.position + firePoint.forward * range;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, targetLayer))
        {
            targetPoint = hit.point;
        }

        SpawnLaserBolt(targetPoint);
    }

    private void SpawnLaserBolt(Vector3 targetPoint)
    {
        if (laserBoltPrefab == null)
            return;

        Vector3 startPosition = muzzlePosition.position;
        float distance = Vector3.Distance(startPosition, targetPoint);
        Quaternion boltRotation = Quaternion.LookRotation(targetPoint - startPosition);

        GameObject bolt = Instantiate(laserBoltPrefab, startPosition, boltRotation);

        if (bolt.TryGetComponent(out LaserBolt laserBolt))
            laserBolt.Initialize(distance, targetLayer, damage, hitForce);
    }

    public override void Reload()
    {
        return;
    }
}
