using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages the Visit/Transfer button UI panel shown when selecting a station
/// Uses singleton pattern for easy access from station selection logic
/// IMPROVEMENT: Add null check for singleton (if Instance exists and != this, destroy)
/// IMPROVEMENT: Consider using events instead of direct method calls
/// CONSISTENCY: Good - uses Awake() for initialization
/// </summary>
public class VisitButtonUI : MonoBehaviour
{
    // Singleton instance for global access
    // IMPROVEMENT: Should be private with public property for encapsulation
    public static VisitButtonUI Instance;

    // Button to visit/explore the selected station
    [SerializeField] private Button visit;

    // Button to transfer between train lines (only enabled at intersections)
    [SerializeField] private Button transfer;

    /// <summary>
    /// Initialize singleton and hide UI at start
    /// CONSISTENCY: Good - correctly uses Awake() for initialization
    /// IMPROVEMENT: Add singleton protection (destroy if duplicate exists)
    /// </summary>
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the button panel and enables/disables transfer based on station type
    /// Called when player selects a station
    /// </summary>
    /// <param name="isIntersection">True if station has multiple train lines (allows transfer)</param>
    /// IMPROVEMENT: Consider passing Station object instead of bool for more flexibility
    public void DisplayButton(bool allowVisit, bool isIntersection)
    {
        gameObject.SetActive(true);
        // Transfer button only works at intersection stations with multiple lines
        visit.interactable = allowVisit;
        transfer.interactable = isIntersection;
    }

    /// <summary>
    /// Hides the button panel
    /// Called when player cancels selection or clicks away
    /// </summary>
    public void HideButton()
    {
        gameObject.SetActive(false);
    }
}
