using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Lection3Controller : MonoBehaviour
{
    [SerializeField]
    private int inputNumber = 0;

    [SerializeField]
    private List<int> numbers = new List<int>();

    [ContextMenu("Add new number")]
    void AddNewItem()
    {
        numbers.Add(inputNumber);
    }

    [ContextMenu("Sort elements by ascending order")]
    void SortAscElements()
    {
        numbers.Sort();
    }

    [ContextMenu("Sort elements by descending order")]
    void SortDescElements()
    {
        numbers.Sort(new SortIntegerDescending());
    }

    [ContextMenu("Remove element from list")]
    void RemoveElementFromList()
    {
        int indexToDelete = numbers.FindIndex(x => x == inputNumber);

        if (indexToDelete != -1)
        {
            numbers.RemoveAt(indexToDelete);
        }
        else
        {
            Debug.LogWarning("Element not found in the list");
        }
    }

    [ContextMenu("Print elements from list")]
    void PrintElementsFromList()
    {
        string elementsString = string.Join("\r\n", numbers);
        Debug.Log($"List of elements:\r\n{elementsString}");
    }

    [ContextMenu("Clear list")]
    void ClearList()
    {
        numbers.Clear();
    }
}
