using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystemManager : MonoBehaviour, IService
{
    private static bool _isLoadPending = false;
    private string _saveFilePath;

    private List<ISaveable> _saveables = new();

    public void RegisterSaveable(ISaveable saveable)
    {
        if (!_saveables.Contains(saveable))
        {
            _saveables.Add(saveable);
        }
        else
        {
            _saveables.Remove(saveable);
            _saveables.Add(saveable);
        }
    }

    public void UnregisterSaveable(ISaveable saveable)
    {
        if (_saveables.Contains(saveable))
        {
            _saveables.Remove(saveable);
        }
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        Debug.Log($"Saving game with {_saveables.Count} saveable objects.");
        foreach (var saveable in _saveables)
        {
            saveData = saveable.GetSaveData(saveData);
        }
        string json = JsonUtility.ToJson(saveData);
        System.IO.File.WriteAllText(_saveFilePath, json);
        Debug.Log($"Game saved at: {_saveFilePath}");
    }

    public void LoadGame()
    {
        if (System.IO.File.Exists(_saveFilePath))
        {
            try
            {
                Debug.Log($"LoadGame called. Saveables count: {_saveables.Count}");
                string json = System.IO.File.ReadAllText(_saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                foreach (var saveable in _saveables)
                {
                    if(saveable as UnityEngine.Object != null)
                        saveable.SetSaveData(saveData);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load save data: {ex.Message}");
            }
        }
    }

    void Awake()
    {
        _saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    void Start()
    {
        if(_isLoadPending)
        {
            _isLoadPending = false;
            StartCoroutine(LoadAfterFrame());
        }
    }

    public bool CheckIfSaveFileExists()
    {
        return System.IO.File.Exists(_saveFilePath);
    }

    public void RequestLoad() => _isLoadPending = true;

    private IEnumerator LoadAfterFrame()
    {
        yield return null;
        LoadGame();
    }
}
