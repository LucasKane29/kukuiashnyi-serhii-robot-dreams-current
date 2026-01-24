using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortIntegerDescending : IComparer<int>
{
    int IComparer<int>.Compare(int a, int b)
    {
        if (a > b)
            return -1;
        if (a < b)
            return 1;
        else
            return 0;
    }
}
