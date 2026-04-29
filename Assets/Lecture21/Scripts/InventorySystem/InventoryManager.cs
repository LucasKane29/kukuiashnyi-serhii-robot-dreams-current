using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSaveEntry
{
    public string itemId;
    public int amount;
}

public class InventoryManager : MonoBehaviour, IService, ISaveable
{
    [SerializeField]
    private EventBus _eventBus;
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private ItemRegistry _itemRegistry;

    private Dictionary<ItemData, int> _items = new ();

    private SaveSystemManager _systemManager;

    void Awake()
    {
        _systemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _systemManager.RegisterSaveable(this);
    }

    void OnDestroy()
    {
        if (_systemManager != null)
        {
            _systemManager.UnregisterSaveable(this);
        }
    }

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

    public SaveData GetSaveData(SaveData data)
    {
        data.items = new List<ItemSaveEntry>();
        foreach (var kvp in _items)
        {
            data.items.Add(new ItemSaveEntry
            {
                itemId = kvp.Key.Id,
                amount = kvp.Value
            });
        }
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        _items = new Dictionary<ItemData, int>();
        foreach (var itemSaveEntry in data.items)
        {
            var itemData = _itemRegistry.GetById(itemSaveEntry.itemId);
            if (itemData != null)
            {
                _items[itemData] = itemSaveEntry.amount;
            }
        }
        _eventBus.Publish(new UpdateInventoryEvent(_items));
        Debug.Log($"SetSaveData called on {gameObject.name}");
    }
}