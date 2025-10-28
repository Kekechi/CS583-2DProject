using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a train station on the map
/// Handles visual representation, mouse interaction, and station data
/// Uses Unity's MonoBehaviour for 3D object interaction
/// IMPROVEMENT: Separate data from behavior (use StationData ScriptableObject)
/// IMPROVEMENT: Consider using new Input System instead of OnMouse* methods
/// IMPROVEMENT: Add validation for required fields in OnValidate()
/// </summary>
public class Station : MonoBehaviour
{
    // Collection of train lines that pass through this station
    // [NonSerialized] prevents Unity from trying to serialize HashSet
    // IMPROVEMENT: Consider making this private with public getter
    [NonSerialized] public HashSet<TrainLine> Lines = new HashSet<TrainLine>();

    // Emission color for station highlight effect
    [SerializeField] private Color emission;

    // 3D text label displaying station name
    [SerializeField] private TextMeshProUGUI label;

    // Audio clip for click feedback
    [SerializeField] private AudioClip click;

    // Station data properties - exposed in Unity Inspector
    // IMPROVEMENT: Group these into a StationData ScriptableObject for better organization
    public string Name;                    // Station display name
    public Sprite ImageSprite;             // Station image for info board
    public string Description;             // Station description text
    public int VisitTime;                  // Minutes required to visit this station
    public int Points;                     // Memory points awarded for visiting
    public string NearestStations;         // Comma-separated nearby station names
    public TourTokyo.Genre genre;          // Station category (for stamp rally system)

    // Cached material reference for emission effects
    private Material material;

    /// <summary>
    /// Cache the material component on initialization
    /// Material is needed for emission highlight effects
    /// CONSISTENCY: Good - uses Awake() for component caching
    /// </summary>
    void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    /// <summary>
    /// Set the 3D label text to station name
    /// IMPROVEMENT: Could be moved to Awake() for consistency
    /// </summary>
    void Start()
    {
        label.text = Name;
    }

    /// <summary>
    /// Checks if this station is an intersection (served by multiple train lines)
    /// Used to determine if transfer button should be enabled
    /// </summary>
    /// <returns>True if station has 2+ train lines</returns>
    /// CONSISTENCY: Good use of descriptive method name
    public bool IsIntersection()
    {
        return Lines.Count > 1;
    }

    /// <summary>
    /// Enables emission glow effect on station material
    /// Called when player hovers over station or it's selectable
    /// IMPROVEMENT: Cache emission keyword string to avoid allocations
    /// </summary>
    public void EnableHighlight()
    {
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
    }

    /// <summary>
    /// Disables emission glow effect on station material
    /// Called when player stops hovering or station becomes unselectable
    /// </summary>
    public void DisableHighlight()
    {
        material.DisableKeyword("_EMISSION");
    }

    /// <summary>
    /// Unity callback when mouse cursor enters this GameObject's collider
    /// Notifies TourTokyo to show station info and highlight
    /// IMPROVEMENT: New Input System would be better for modern Unity projects
    /// IMPROVEMENT: Add null check for TourTokyo.Instance
    /// </summary>
    void OnMouseEnter()
    {
        TourTokyo.Instance.OnHoverEnterStation(this);
    }

    /// <summary>
    /// Unity callback when mouse cursor exits this GameObject's collider
    /// Notifies TourTokyo to hide station info and remove highlight
    /// IMPROVEMENT: Add null check for TourTokyo.Instance
    /// </summary>
    void OnMouseExit()
    {
        TourTokyo.Instance.OnHoverExitStation(this);
    }

    /// <summary>
    /// Unity callback when mouse button is pressed down on this GameObject
    /// Plays click sound and notifies TourTokyo of station selection
    /// IMPROVEMENT: Add null checks for click and TourTokyo.Instance
    /// IMPROVEMENT: Consider separating audio from selection logic
    /// </summary>
    void OnMouseDown()
    {
        AudioController.PlayClip(click);
        TourTokyo.Instance.SelectStation(this);
    }
}
