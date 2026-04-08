using UnityEngine;


public enum ItemType 
{
    Weapon, 
    Consumable,
    Money
}

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public abstract class ItemData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public Sprite Icon;
    public ItemType Type;
    public int MaxStackSize = 1;

    public abstract void Use(GameObject player);
}