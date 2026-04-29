using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantOfferItem : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;

    private MerchantItems _offer;
    private MerchantController _merchant;
    private LocalizationManager _localizationManager;
    private bool _isBuy;

    public void Setup(MerchantItems offer, bool isBuy, MerchantController merchant, LocalizationManager localizationManager)
    {
        _offer = offer;
        _isBuy = isBuy;
        _merchant = merchant;
        _localizationManager = localizationManager;
        _itemIcon.sprite = offer.itemData.Icon;
        _itemName.text = _localizationManager.GetLocalizedValue(offer.itemData.DisplayName);
        _price.text = $"{offer.price} c.";

        _actionButton.onClick.RemoveAllListeners();
        _actionButton.onClick.AddListener(OnActionClicked);
        _buttonText.text = isBuy ? _localizationManager.GetLocalizedValue("BuyButtonText") : _localizationManager.GetLocalizedValue("SellButtonText");
    }

    private void OnActionClicked()
    {
        if (_isBuy)
            _merchant.BuyItem(_offer);
        else
            _merchant.SellItem(_offer);
    }
}
