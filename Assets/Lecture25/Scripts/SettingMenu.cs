using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingMenu : MonoBehaviour
{
    private LocalizationManager _localizationManager;

    private List<Language> _availableLanguages;

    [SerializeField]
    private TMP_Dropdown _languageDropdown;

    private int _selectedIndex;

    public void Start()
    {
        _localizationManager = IServiceLocator.Instance.GetService<LocalizationManager>();
        _availableLanguages = _localizationManager.GetAvailableLanguages();
        _languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
        string _currentLanguageCode = _localizationManager.GetCurrentLanguage();
        _selectedIndex = _availableLanguages.FindIndex(lang => lang.Code == _currentLanguageCode);
        ReloadDropdown(_selectedIndex);
    }

    public void OnLanguageSelected(int index)
    {
        if (index >= 0 && index < _availableLanguages.Count)
        {
            _selectedIndex = index;
            var selectedLanguage = _availableLanguages[index];
            _localizationManager.LoadLanguage(selectedLanguage.Code);
            ReloadDropdown(_selectedIndex);
        }
    }

    private void ReloadDropdown(int selectedIndex)
    {
        _languageDropdown.Hide();
        _languageDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var option in _availableLanguages)
        {
            options.Add(new TMP_Dropdown.OptionData(_localizationManager.GetLocalizedValue(option.LabelText)));
        }
        _languageDropdown.AddOptions(options);
        _languageDropdown.SetValueWithoutNotify(selectedIndex);
        _languageDropdown.RefreshShownValue();
    }
}
