using UnityEngine;

public interface IWeapon
{
    bool CanFire { get; }
    void Fire(Transform firePoint);
    void Reload();
}
