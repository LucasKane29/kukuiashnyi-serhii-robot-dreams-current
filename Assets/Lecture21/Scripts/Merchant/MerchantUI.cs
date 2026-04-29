using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MerchantUI : MonoBehaviour
{
    [SerializeField] private Transform _forSaleContainer;
    [SerializeField] private Transform _toBuyContainer;
    [SerializeField] private MerchantOfferItem _offerRowPrefab;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _merchantText;
    private LocalizationManager _localizationManager;

    private List<MerchantOfferItem> _rows = new();

    private void Awake()
    {
        _localizationManager = IServiceLocator.Instance.GetService<LocalizationManager>();

    }

    public void Show(MerchantController merchant)
    {
        gameObject.SetActive(true);
        Populate(merchant);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearRows();
    }

    private void Populate(MerchantController merchant)
    {
        ClearRows();

        foreach (var offer in merchant.ItemsForSale)
        {
            var row = Instantiate(_offerRowPrefab, _forSaleContainer);
            row.Setup(offer, isBuy: true, merchant, _localizationManager);
            _rows.Add(row);
        }

        foreach (var offer in merchant.ItemsToBuy)
        {
            var row = Instantiate(_offerRowPrefab, _toBuyContainer);
            row.Setup(offer, isBuy: false, merchant, _localizationManager);
            _rows.Add(row);
        }
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            Destroy(row.gameObject);
        _rows.Clear();
    }

    public void ShowMessage(string message)
    {
        _merchantText.text = message;
    }
}
