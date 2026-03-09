using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeWeapon : Weapon
{
    [SerializeField] private Rigidbody grenadePrefab;
    [SerializeField] private Transform startPosition;
    [SerializeField] private float launchForce = 20f;


    public override void Fire()
    {
        if (!CanFire || grenadePrefab == null) return;

        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;

        Rigidbody grenade = Instantiate(grenadePrefab, startPosition.position, startPosition.rotation);
        grenade.AddForce(startPosition.forward * launchForce, ForceMode.Impulse);

        if (grenade.TryGetComponent(out Grenade grenadeScript))
            grenadeScript.Init(damage, targetLayer);
    }
}
