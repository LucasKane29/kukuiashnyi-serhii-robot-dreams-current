using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IService
{
    [SerializeField]
    private EventBus _eventBus;
    [SerializeField]
    private GameObject _player;

    private Dictionary<ItemData, int> _items = new ();

    public bool AddItem(ItemData item, int quantity)
    {
        if (_items.TryGetValue(item, out int currentQuantity))
        {
            if (currentQuantity + quantity > item.MaxStackSize)
            {
                return false; 
            }
            _items[item] = currentQuantity + quantity;
        }
        else
        {
            if (quantity > item.MaxStackSize)
            {
                return false; 
            }
            _items[item] = quantity;
        }
        _eventBus.Publish(new UpdateInventoryEvent(_items));
        return true;
    }

    public bool RemoveItem(ItemData item, int quantity)
    {
        if (_items.TryGetValue(item, out int currentQuantity))
        {
            if (currentQuantity < quantity)
            {
                return false; 
            }
            _items[item] = currentQuantity - quantity;
            if (_items[item] == 0)
            {
                _items.Remove(item);
            }
            _eventBus.Publish(new UpdateInventoryEvent(_items));
            return true;
        }
        return false;
    }

    public int GetItemQuantity(ItemData item)
    {
        if (_items.TryGetValue(item, out int quantity))
        {
            return quantity;
        }
        return 0;
    }

    public bool HasItem(ItemData item)
    {
        return _items.ContainsKey(item);
    }

    public IReadOnlyDictionary<ItemData, int> GetItems()
    {
        return _items; 
    }

    public void UseItem(ItemData item)
    {
        if (HasItem(item))
        {
            item.Use(_player);
            if(item.Type == ItemType.Consumable)
            {
                RemoveItem(item, 1);
                _eventBus.Publish(new UpdateInventoryEvent(_items));
            }
        }
    }
}
