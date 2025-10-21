using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoryPointUI : MonoBehaviour
{
    private TextMeshProUGUI text;
    private int _points = 0;
    public static MemoryPointUI Instance;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public int Points
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
