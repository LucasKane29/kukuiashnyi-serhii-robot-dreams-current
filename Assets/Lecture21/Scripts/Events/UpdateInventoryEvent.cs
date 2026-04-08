using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UpdateInventoryEvent
{

    public Dictionary<ItemData, int> Items { get; private set; }

    public UpdateInventoryEvent(Dictionary<ItemData, int> items)
    {
        this.Items = items;
    }
}
