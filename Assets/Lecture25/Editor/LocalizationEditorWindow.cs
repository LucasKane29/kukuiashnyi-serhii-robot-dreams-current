using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class LocalizationEditorWindow : EditorWindow
{
    private List<string> _languages = new();
    private Dictionary<string, LocalizationData> _allTranslations = new();
    private List<string> _keys = new();

    private Vector2 _scrollPos;
    private string _newKey = "";
    private string _newLangCode = "";
    private string _searchFilter = "";
    private string _localizationPath;

    [MenuItem("Tools/Localization Editor")]
    public static void Open()
    {
        var window = GetWindow<LocalizationEditorWindow>("Localization");
        window.minSize = new Vector2(600, 400);
        window.LoadAll();
    }

    private void OnEnable()
    {
        _localizationPath = Path.Combine(Application.dataPath, "Resources", "Localization");
        LoadAll();
    }

    private void LoadAll()
    {
        _languages.Clear();
        _allTranslations.Clear();
        _keys.Clear();

        if (!Directory.Exists(_localizationPath))
        {
            Directory.CreateDirectory(_localizationPath);
            return;
        }

        foreach (var file in Directory.GetFiles(_localizationPath, "*.json"))
        {
            var langCode = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<LocalizationData>(json) ?? new LocalizationData();

            _languages.Add(langCode);
            _allTranslations[langCode] = data;

            foreach (var key in data.ToDictionary().Keys)
            {
                if (!_keys.Contains(key))
                    _keys.Add(key);
            }
        }

        _keys.Sort();
    }

    private void SaveAll()
    {
        foreach (var lang in _languages)
        {
            var path = Path.Combine(_localizationPath, $"{lang}.json");
            var json = JsonConvert.SerializeObject(_allTranslations[lang], Formatting.Indented);
            File.WriteAllText(path, json);
        }

        AssetDatabase.Refresh();
        Debug.Log("Localization saved!");
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawAddKeyRow();
        DrawTable();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            SaveAll();

        if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
            LoadAll();

        GUILayout.FlexibleSpace();

        // Пошук
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        _searchFilter = EditorGUILayout.TextField(_searchFilter,
                         EditorStyles.toolbarSearchField, GUILayout.Width(200));

        GUILayout.Space(10);

        // Додати мову
        _newLangCode = EditorGUILayout.TextField(_newLangCode, GUILayout.Width(60));
        if (GUILayout.Button("+Lang", EditorStyles.toolbarButton, GUILayout.Width(50)))
            AddLanguage();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawAddKeyRow()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New key:", GUILayout.Width(60));
        _newKey = EditorGUILayout.TextField(_newKey);
        if (GUILayout.Button("Add Key", GUILayout.Width(80)))
            AddKey();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
    }

    private void DrawTable()
    {
        if (_languages.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No languages found. Add a language code above (e.g. 'en', 'uk').",
                MessageType.Info);
            return;
        }

        // Заголовки стовпців
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(180));
        foreach (var lang in _languages)
            EditorGUILayout.LabelField(lang.ToUpper(), EditorStyles.boldLabel);
        GUILayout.Space(30); // місце для кнопки видалення
        EditorGUILayout.EndHorizontal();

        // Рядки таблиці
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        var keysToRemove = new List<string>();

        foreach (var key in _keys)
        {
            // Фільтр пошуку
            if (!string.IsNullOrEmpty(_searchFilter) &&
                !key.Contains(_searchFilter, System.StringComparison.OrdinalIgnoreCase))
                continue;

            EditorGUILayout.BeginHorizontal();

            // Ключ (read-only)
            EditorGUILayout.SelectableLabel(key, EditorStyles.textField,
                                             GUILayout.Width(180), GUILayout.Height(20));

            // Значення для кожної мови
            foreach (var lang in _languages)
            {
                _allTranslations[lang].ToDictionary().TryGetValue(key, out var value);
                var newValue = EditorGUILayout.TextField(value ?? "");
                int i = System.Array.FindIndex(_allTranslations[lang].entries, e => e.key == key);

                if(i == -1) { 
                    i = _allTranslations[lang].entries.Length;
                }
                _allTranslations[lang].entries[i] = new LocalizationEntry { key = key, value = newValue };
            }

            // Кнопка видалення
            if (GUILayout.Button("×", GUILayout.Width(24)))
                keysToRemove.Add(key);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Видалення ключів
        foreach (var key in keysToRemove)
            RemoveKey(key);
    }

    private void AddLanguage()
    {
        var code = _newLangCode.Trim().ToLower();
        if (string.IsNullOrEmpty(code) || _languages.Contains(code))
            return;

        _languages.Add(code);
        _allTranslations[code] = new LocalizationData();

        // Заповнити порожніми значеннями для існуючих ключів
        foreach (var key in _keys)
        {
            if(_allTranslations[code].entries == null)
                _allTranslations[code].entries = new[] { new LocalizationEntry { key = key, value = "" } };
            else
            {
                var entries = _allTranslations[code].entries;
                System.Array.Resize(ref entries, entries.Length + 1);
                entries[entries.Length - 1] = new LocalizationEntry { key = key, value = "" };
                _allTranslations[code].entries = entries;
            }
        }


        _newLangCode = "";
    }

    private void AddKey()
    {
        var key = _newKey.Trim();
        if (string.IsNullOrEmpty(key) || _keys.Contains(key))
            return;

        _keys.Add(key);
        _keys.Sort();

        foreach (var lang in _languages)
        {
            var entries = _allTranslations[lang].entries;
            System.Array.Resize(ref entries, entries.Length + 1);
            entries[entries.Length - 1] = new LocalizationEntry { key = key, value = "" };
            _allTranslations[lang].entries = entries;
        }

        _newKey = "";
    }

    private void RemoveKey(string key)
    {
        _keys.Remove(key);
        foreach (var lang in _languages)
        {
            var entries = _allTranslations[lang].entries;
            _allTranslations[lang].entries = System.Array.FindAll(entries, e => e.key != key);
        }
    }
}