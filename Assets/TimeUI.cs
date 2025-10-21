using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    private static TextMeshProUGUI text;
    private static int _gameTime = 0;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public static int GameTime
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
