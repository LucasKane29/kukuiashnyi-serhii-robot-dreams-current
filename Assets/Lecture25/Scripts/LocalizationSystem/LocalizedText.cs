using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField]
    private string _key;

    private TMPro.TextMeshProUGUI _textComponent;

    void Awake()
    {
        _textComponent = GetComponent<TMPro.TextMeshProUGUI>();
        IServiceLocator.Instance.GetService<LocalizationManager>().OnLanguageChanged += OnLocalizationChanged;
    }

    void OnEnable()
    {
        OnLocalizationChanged();
    }

    private void OnLocalizationChanged()
    {
        var localizedValue = IServiceLocator.Instance?.GetService<LocalizationManager>().GetLocalizedValue(_key);
        _textComponent.text = localizedValue;
    }

    private void OnDestroy()
    {
        var manager = IServiceLocator.Instance?.GetService<LocalizationManager>();
        if (manager != null)
            manager.OnLanguageChanged -= OnLocalizationChanged;
    }
}
