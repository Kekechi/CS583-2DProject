using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    public static TimeUI Instance;
    private TextMeshProUGUI text;
    private int _gameTime = 0;

    void Awake()
    {
        Instance = this;
        text = GetComponent<TextMeshProUGUI>();
    }

    public int GameTime
    {
        get => _gameTime;
        set
        {
            if (_gameTime == value)
            {
                return;
            }
            _gameTime = value;
            text.text = FormatTime(value);
        }
    }

    static string FormatTime(int time)
    {
        return $"{(time / 60):00}:{(time % 60):00}";
    }
}
