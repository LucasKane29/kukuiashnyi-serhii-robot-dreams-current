using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MerchantItems
{
    public ItemData itemData;
    public int price;
}

public class MerchantController : MonoBehaviour, IService
{
    [SerializeField]
    private CoinItemData _coinData;

    [Header("Що мерчант продає гравцю")]
    [SerializeField]
    private List<MerchantItems> _itemsForSale;

    [Header("Що мерчант купує у гравця")]
    [SerializeField]
    private List<MerchantItems> _itemsToBuy;

    [SerializeField]
    private MerchantUI _merchantUI;

    [SerializeField]
    private Transform _interactButton;

    [SerializeField]
    private string _playerTag = "Player";

    private InventoryManager _inventoryManager;
    private GameManager _gameManager;
    private LocalizationManager _localizationManager;

    public IReadOnlyList<MerchantItems> ItemsForSale => _itemsForSale;
    public IReadOnlyList<MerchantItems> ItemsToBuy => _itemsToBuy;

    private bool _isCanInteract = false;


    private void Awake()
    {
        _inventoryManager = IServiceLocator.Instance.GetService<InventoryManager>();
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
        _localizationManager = IServiceLocator.Instance.GetService<LocalizationManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _interactButton.gameObject.SetActive(true);
        _isCanInteract = true;
        _gameManager.SetCanInteract(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _interactButton.gameObject.SetActive(false);
        _isCanInteract = false;
        _merchantUI.Hide();
        _gameManager.SetCanInteract(false);
    }

    public void OpenShop()
    {
        if (_isCanInteract)
        {
            _merchantUI.Show(this);
        }
    }

    public void CloseShop()
    {
        _merchantUI.Hide();
    }

    public bool BuyItem(MerchantItems offer)
    {
        if (_inventoryManager.GetItemQuantity(_coinData) < offer.price)
        {
            _merchantUI.ShowMessage(_localizationManager.GetLocalizedValue("NotEnoughCoins", offer.price));
            return false;
        }

        bool removed = _inventoryManager.RemoveItem(_coinData, offer.price);
        if (!removed) return false;

        bool added = _inventoryManager.AddItem(offer.itemData, 1);
        if (!added)
        {
            _inventoryManager.AddItem(_coinData, offer.price);
            _merchantUI.ShowMessage(_localizationManager.GetLocalizedValue("InventoryFull"));
            return false;
        }
        _merchantUI.ShowMessage(_localizationManager.GetLocalizedValue("BoughtItem", _localizationManager.GetLocalizedValue(offer.itemData.DisplayName), offer.price));
        return true;
    }

    public bool SellItem(MerchantItems offer)
    {
        if (_inventoryManager.GetItemQuantity(offer.itemData) < 1)
        {
            _merchantUI.ShowMessage(_localizationManager.GetLocalizedValue("NoItemToSell", _localizationManager.GetLocalizedValue(offer.itemData.DisplayName)));
            return false;
        }

        bool removed = _inventoryManager.RemoveItem(offer.itemData, 1);
        if (!removed) return false;

        _inventoryManager.AddItem(_coinData, offer.price);
        _merchantUI.ShowMessage(_localizationManager.GetLocalizedValue("SoldItem", _localizationManager.GetLocalizedValue(offer.itemData.DisplayName), offer.price));
        return true;
    }

    public void OnCloseButton()
    {
        CloseShop();
        _gameManager.ResumeGame();
    }
}
