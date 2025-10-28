using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the player character on the train map
/// Handles player positioning, movement animation between stations
/// IMPROVEMENT: Separate movement logic from player data (use a PlayerController)
/// IMPROVEMENT: Add more movement types (curved paths, easing functions)
/// IMPROVEMENT: Consider using DOTween for smoother animations
/// </summary>
public class Player : MonoBehaviour
{
    // The station GameObject where player is currently located
    // Public getter, private setter for encapsulation
    // CONSISTENCY: Good use of property with private setter
    public GameObject CurrentStation { get; private set; }

    // Offset to position player slightly above/away from station point
    [SerializeField] private Vector3 playerStationOffset;

    // Duration in seconds for movement animation between stations
    [SerializeField] private float duration = 1f;

    /// <summary>
    /// Sets player's initial position at the starting station
    /// Called once at game start by TourTokyo
    /// </summary>
    /// <param name="startStation">The starting station GameObject</param>
    /// IMPROVEMENT: Add null check for startStation parameter
    public void Initialize(GameObject startStation)
    {
        CurrentStation = startStation;
        transform.position = startStation.transform.position + playerStationOffset;
    }

    /// <summary>
    /// Initiates animated movement from current station to destination
    /// Starts coroutine to handle smooth interpolation over time
    /// </summary>
    /// <param name="station">Destination station GameObject</param>
    /// <param name="timeCost">In-game time cost for travel (in minutes)</param>
    /// <param name="onComplete">Callback invoked when movement completes</param>
    /// IMPROVEMENT: Add validation that station is reachable from current position
    /// IMPROVEMENT: Consider cancellation support if player selects new destination mid-travel
    public void SelectDestination(GameObject station, int timeCost, Action onComplete)
    {
        StartCoroutine(AnimateMovement(station, timeCost, onComplete));
    }

    /// <summary>
    /// Coroutine that smoothly animates player movement to target station
    /// Uses linear interpolation (Lerp) for position
    /// Updates game time proportionally during movement
    /// </summary>
    /// <param name="targetStation">Destination station GameObject</param>
    /// <param name="timeCost">In-game minutes to travel</param>
    /// <param name="onComplete">Callback to invoke upon completion</param>
    /// IMPROVEMENT: Use easing curves (AnimationCurve) for more natural movement
    /// IMPROVEMENT: Consider path following for curved routes
    IEnumerator AnimateMovement(GameObject targetStation, int timeCost, Action onComplete)
    {
        // Store starting position and calculate target position with offset
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetStation.transform.position + playerStationOffset;

        // Interpolation parameter (0 to 1)
        float t = 0f;

        // Store starting time to calculate elapsed in-game time
        int prevTime = TimeUI.GameTime;

        // Animate movement until reaching destination (t = 1)
        while (t < 1f)
        {
            // Increment t based on real time and animation duration
            t += Time.deltaTime / duration;

            // Update in-game time proportionally to movement progress
            TimeUI.GameTime = prevTime + (int)(t * timeCost);

            // Linearly interpolate position from start to target
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // Wait one frame before continuing
            yield return null;
        }

        // Ensure final values are exact (avoid floating point errors)
        TimeUI.GameTime = prevTime + timeCost;
        CurrentStation = targetStation;
        transform.position = targetPos;

        // Notify completion (null-conditional operator handles null callbacks safely)
        onComplete?.Invoke();
    }
}
