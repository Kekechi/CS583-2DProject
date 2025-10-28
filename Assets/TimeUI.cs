using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays in-game time in HH:MM format
/// Uses static property pattern similar to MemoryPointUI for easy access
/// IMPROVEMENT: Add null check for 'text' in property setter
/// IMPROVEMENT: Separate time logic from UI (use a TimeManager class)
/// IMPROVEMENT: Consider using events to notify when time changes
/// CONSISTENCY: Good - uses Awake() like recommended, unlike MemoryPointUI
/// </summary>
public class TimeUI : MonoBehaviour
{
    // Static reference to TextMeshPro component for updating display
    // IMPROVEMENT: Should be instance field, not static
    private static TextMeshProUGUI text;

    // Backing field for game time in minutes
    private static int _gameTime = 0;

    /// <summary>
    /// Gets the TextMeshPro component on initialization
    /// CONSISTENCY: Correctly uses Awake() for component initialization
    /// </summary>
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Static property for reading/writing game time in minutes
    /// Automatically updates UI with formatted time (HH:MM) when value changes
    /// IMPROVEMENT: Add null check before accessing 'text'
    /// IMPROVEMENT: Consider instance-based approach with singleton
    /// </summary>
    public static int GameTime
    {
        get => _gameTime;
        set
        {
            // Early return if value hasn't changed (optimization)
            if (_gameTime == value)
            {
                return;
            }
            _gameTime = value;
            // Update UI with formatted time string
            text.text = FormatTime(value);
        }
    }

    /// <summary>
    /// Converts time in minutes to HH:MM format string
    /// Example: 570 minutes (9:30 AM) -> "09:30"
    /// IMPROVEMENT: Could be moved to a utility class for reuse
    /// </summary>
    static string FormatTime(int timeInMinutes)
    {
        int hours = timeInMinutes / 60;
        int minutes = timeInMinutes % 60;
        return $"{hours:00}:{minutes:00}";
    }
}
