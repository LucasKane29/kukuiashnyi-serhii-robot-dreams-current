using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float playerHealth;
    public ScoreData scoreData;
    public Vector3 playerPosition;
    public List<ItemSaveEntry> items;
    public SoundSettingsData soundSettings;
    public LocalizationSettingsData localizationSettings;
}