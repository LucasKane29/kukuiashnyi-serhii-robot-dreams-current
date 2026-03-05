using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 10f;

    [SerializeField] protected float fireRate = 1f;

    [SerializeField] protected int maxAmmo = 10;
    [SerializeField] protected float reloadTime = 2f;

    protected int currentAmmo;
    protected float nextFireTime;
    protected bool isReloading;

    public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= nextFireTime;

    protected virtual void Awake()
    {
        currentAmmo = maxAmmo;
    }

    public abstract void Fire(Transform firePoint);

    public virtual void Reload()
    {
        if (!isReloading && currentAmmo < maxAmmo)
            StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
