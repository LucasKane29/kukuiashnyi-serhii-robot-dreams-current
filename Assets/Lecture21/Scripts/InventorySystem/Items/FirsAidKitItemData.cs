using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/FirsAidKitItemData")]
public class FirstAidKitItemData : ItemData
{
    public GameObject firstAidKitPrefab;
    public float healLevel;

    public override void Use(GameObject player)
    {
        IHealable healable = player.GetComponent<IHealable>();
        if (healable != null)
        {
            healable.Heal(healLevel);
        }
    }
}