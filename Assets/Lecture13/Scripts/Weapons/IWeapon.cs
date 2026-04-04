using UnityEngine;

public interface IWeapon
{
    bool CanFire { get; }
    void Fire(Vector3? targetDirection = null);
    void Reload();
}
