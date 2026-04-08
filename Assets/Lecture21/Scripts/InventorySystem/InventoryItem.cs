using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField]
    private Image _itemImage;

    [SerializeField]
    private TextMeshProUGUI _itemCountText;

    private ItemData _itemData;

    private Sprite _itemIcon;
    private int _itemCount;



    public void SetItem(ItemData itemData, int itemCount)
    {
        _itemCount = itemCount;
        _itemData = itemData;
        _itemIcon = itemData.Icon;
        UpdateUI();
    }

    private void UpdateUI()
    {
        _itemImage.sprite = _itemIcon;
        _itemCountText.text = _itemCount.ToString();
    }

    public void OnItemClicked()
    {
        if (_itemData != null)
        {
            InventoryManager inventoryManager = IServiceLocator.Instance.GetService<InventoryManager>();
            inventoryManager.UseItem(_itemData);
        }
    }
}
