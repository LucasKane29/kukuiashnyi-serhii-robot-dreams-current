using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Language
{
    public string Code;
    public string LabelText;
}

public class LocalizationManager : MonoBehaviour, IService, ISaveable
{
    [SerializeField]
    private string _defaultLanguage;

    [SerializeField]
    private List<Language> _langs;

    private Dictionary<string, string> _localizedText;
    public event Action OnLanguageChanged;
    private string _currentLanguage;
    private SaveSystemManager _saveSystemManager;

    void Awake()
    {
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _saveSystemManager.RegisterSaveable(this);

        if(_currentLanguage == null)
            _currentLanguage = _defaultLanguage;    
        LoadLanguage(_currentLanguage);
    }

    private void OnDestroy()
    {
        if (_saveSystemManager != null)
        {
            _saveSystemManager.UnregisterSaveable(this);
        }
    }
    public void LoadLanguage(string langCode)
    {
        var jsonContent = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (jsonContent == null)
        {
            Debug.LogError($"Localization file not found for language code: {langCode}");
            return;
        }

        _localizedText = JsonUtility.FromJson<LocalizationData>(jsonContent.text).ToDictionary();
        Resources.UnloadAsset(jsonContent);
        _currentLanguage = langCode;
        OnLanguageChanged?.Invoke();
    }
    
    public string GetLocalizedValue(string key, params object[] args)
    {
        if (_localizedText != null && _localizedText.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }
        return $"[Missing: {key}]";
    }

    public string GetCurrentLanguage()
    {
        return _currentLanguage;
    }

    public List<Language> GetAvailableLanguages()
    {
        return _langs;
    }

    public SaveData GetSaveData(SaveData data)
    {
        data.localizationSettings = new LocalizationSettingsData();
        data.localizationSettings.localization = _currentLanguage;
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        LoadLanguage(data.localizationSettings.localization);
    }
}
