using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/CoinItemData")]
public class CoinItemData : ItemData
{
    public GameObject coinPrefab;
    public float currencyValue;

    public override void Use(GameObject player)
    {
        return;
    }
}