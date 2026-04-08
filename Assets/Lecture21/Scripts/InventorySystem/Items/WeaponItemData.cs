using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/WeaponItemData")]
[RequireComponent(typeof(Weapon))]
public class WeaponItemData : ItemData
{
    public GameObject WeaponPrefab;
    public float Damage;
    public float FireRate;

    public override void Use(GameObject player)
    {
        PlayerFireController fireController = player.GetComponent<PlayerFireController>();
        if(fireController != null)
        {
            fireController.EquipWeapon(this);
        }
    }
}