using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages display of memory points (game score) in the UI
/// Uses static property pattern for easy score updates from anywhere
/// IMPROVEMENT: Add null check for 'text' in property setter to avoid NullReferenceException
/// IMPROVEMENT: Use Awake() instead of Start() for component initialization
/// IMPROVEMENT: Consider using events to notify listeners when points change
/// IMPROVEMENT: Separate data (points) from presentation (UI text) for better architecture
/// </summary>
public class MemoryPointUI : MonoBehaviour
{
    // Static reference to TextMeshPro component for updating display
    // IMPROVEMENT: This should be instance field, not static
    private static TextMeshProUGUI text;

    // Backing field for memory points
    private static int _points = 0;

    /// <summary>
    /// Gets the TextMeshPro component on initialization
    /// IMPROVEMENT: Change to Awake() to ensure text is set before any Start() methods
    /// </summary>
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        _points = 0;
    }

    /// <summary>
    /// Static property for reading/writing memory points
    /// Automatically updates UI text when value changes
    /// IMPROVEMENT: Add null check - if (text == null) return;
    /// IMPROVEMENT: Consider instance-based approach with singleton instead of static fields
    /// IMPROVEMENT: Fire an event when points change for other systems to react
    /// </summary>
    public static int Points
    {
        get => _points;
        set
        {
            // Early return if value hasn't changed (optimization)
            if (_points == value)
            {
                return;
            }
            _points = value;
            // Update UI text with formatted points
            text.text = $"MemoryPoint: {_points:0}";
        }
    }
}
