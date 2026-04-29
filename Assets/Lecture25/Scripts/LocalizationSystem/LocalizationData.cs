using System.Collections.Generic;

[System.Serializable]
public class LocalizationData
{
    public LocalizationEntry[] entries;

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            dict[entry.key] = entry.value;
        }
        return dict;
    }


}

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}