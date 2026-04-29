using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField] private float radius = 2f;
    public override void Fire(Vector3? targetDirection = null)
    {
        if (!CanFire)
            return;

        nextFireTime = Time.time + (1f / fireRate);

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, radius, targetLayer);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(damage, collider.transform.position, this.transform.position);
        }
        PlayFireSound();
    }

    public override void Reload()
    {
        return;
    }
}
