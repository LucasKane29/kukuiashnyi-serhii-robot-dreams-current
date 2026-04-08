using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemRegistry")]
public class ItemRegistry : ScriptableObject
{
    public List<ItemData> Items;

    public ItemData GetById(string id)
        => Items.Find(item => item.Id == id);
}