using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// Displays detailed information about a station when player hovers/selects it
/// Shows station name, image, travel time, visit time, nearby stations, and train lines
/// Uses singleton pattern for easy access from station selection logic
/// IMPROVEMENT: Add null check for singleton pattern
/// IMPROVEMENT: Use data class/ScriptableObject for station info instead of many parameters
/// </summary>
public class InfoBoard : MonoBehaviour
{
    // UI text elements for displaying station information
    [SerializeField] private TextMeshProUGUI nameText;           // Station name
    [SerializeField] private Image infoImage;                     // Station image/photo
    [SerializeField] private TextMeshProUGUI time;                // Travel time to reach
    [SerializeField] private TextMeshProUGUI visitTime;           // Time required to visit + optional transfer time
    [SerializeField] private TextMeshProUGUI lines;               // Train lines serving this station
    [SerializeField] private TextMeshProUGUI description;         // Station description text
    [SerializeField] private TextMeshProUGUI stations;            // Nearby stations list

    // Singleton instance for global access
    // IMPROVEMENT: Should be private with public property
    public static InfoBoard Instance;

    /// <summary>
    /// Initialize singleton and hide info board at start
    /// CONSISTENCY: Good - uses Awake() for initialization
    /// IMPROVEMENT: Add singleton protection
    /// </summary>
    void Awake()
    {
        Instance = this;
        DisableDisplay();
    }

    /// <summary>
    /// Populates and displays all station information in the info board
    /// Called when player hovers over or selects a station
    /// </summary>
    /// <param name="name">Station name</param>
    /// <param name="image">Station image sprite</param>
    /// <param name="timeCost">Travel time to reach station (-1 if unreachable)</param>
    /// <param name="visitTime">Time required to visit the station</param>
    /// <param name="stationNames">Comma-separated list of nearby stations</param>
    /// <param name="lineNames">Comma-separated list of train lines</param>
    /// <param name="newDescription">Station description text</param>
    /// <param name="transferTime">Optional transfer time if switching lines</param>
    /// IMPROVEMENT: Replace many parameters with StationData class or struct
    /// IMPROVEMENT: Add null checks for string parameters
    public void DisplayStationInfo(string name, Sprite image, int timeCost, int visitTime, string stationNames, string lineNames, string newDescription, int transferTime = -1)
    {
        // Populate all UI text fields with station information
        nameText.text = name;
        infoImage.sprite = image;
        stations.text = $"Nearest Stations: {stationNames}";
        lines.text = $"Train Lines: {lineNames}";
        // Show "Unreachable" message if player can't reach station without transferring
        time.text = timeCost == -1 ? "Unreachable without Transfer" : $"Estimated Time: <b>{timeCost} min</b>";
        // Display visit time, optionally including transfer time
        this.visitTime.text = $"Visit Time: <b>{visitTime} min</b>{((transferTime == -1) ? "" : $"  Transfer Time: <b>{transferTime} min</b>")}";
        description.text = newDescription;
        // Show the info board
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the info board UI panel
    /// Called when player moves mouse away or deselects station
    /// </summary>
    public void DisableDisplay()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Utility method to convert HashSet of TrainLine objects to comma-separated string
    /// Used to format train line names for display
    /// </summary>
    /// <param name="lines">HashSet of TrainLine objects</param>
    /// <returns>Comma-separated string of line names</returns>
    /// IMPROVEMENT: Consider moving to a utility class for reuse
    /// IMPROVEMENT: Could add sorting for consistent display order
    public static string LineSetToString(HashSet<TrainLine> lines)
    {
        return String.Join(", ", lines.Select(x => x.LineName));
    }
}
