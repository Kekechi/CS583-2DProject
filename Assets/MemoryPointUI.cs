using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoryPointUI : MonoBehaviour
{
    private static TextMeshProUGUI text;
    private static int _points = 0;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public static int Points
    {
        get => _points;
        set
        {
            if (_points == value)
            {
                return;
            }
            _points = value;
            text.text = $"MemoryPoint: {_points:0}";
        }
    }
}
