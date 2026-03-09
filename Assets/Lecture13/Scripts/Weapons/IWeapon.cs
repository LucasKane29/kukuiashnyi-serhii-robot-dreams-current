using UnityEngine;

public interface IWeapon
{
    bool CanFire { get; }
    void Fire();
    void Reload();
}
