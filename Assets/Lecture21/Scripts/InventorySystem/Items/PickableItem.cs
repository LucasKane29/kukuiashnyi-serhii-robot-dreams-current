using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [SerializeField] 
    private ItemData _data;
    [SerializeField] 
    private int _quantity;
    [SerializeField]
    private LayerMask _whoCanPick;

    private void OnTriggerEnter(Collider other)
    {
        if((_whoCanPick.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        else
        {
            InventoryManager inventoryManager = other.GetComponent<InventoryManager>();
            if (inventoryManager != null)
            {
                bool added = inventoryManager.AddItem(_data, _quantity);
                if (added)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Not enough space in inventory to pick up item.");
                }
            }
        }
    }
}
