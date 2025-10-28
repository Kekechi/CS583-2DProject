using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game controller that manages the Tour Tokyo game flow
/// Handles game state, player actions, station selection, travel, and time management
/// Uses singleton pattern for global access from UI and game objects
/// IMPROVEMENT: This class is too large - consider splitting into:
///   - GameStateManager (state transitions)
///   - TravelController (pathfinding and movement)
///   - UIManager (UI coordination)
/// IMPROVEMENT: Use UnityEvents for state changes instead of direct method calls
/// CONSISTENCY: Good use of enum for game states
/// </summary>
public class TourTokyo : MonoBehaviour
{
    // Singleton instance for global access
    // CONSISTENCY: Good - uses property with private setter instead of public field
    public static TourTokyo Instance { get; private set; }

    /// <summary>
    /// Enum defining all possible game states
    /// Used to control which player actions are allowed
    /// CONSISTENCY: Good - clear state machine pattern
    /// </summary>
    public enum GameState { GameStart, IdleOnStation, StationSelected, Moving, Arrived, GameEnd }

    /// <summary>
    /// Enum defining station genre categories for stamp rally system
    /// CONSISTENCY: Good - matches StampRallyUI.GenreStamp.Genre
    /// </summary>
    public enum Genre { Landmark, Culture, Museum, Nature, Shopping };

    // Current game state - publicly readable, privately writable
    // CONSISTENCY: Good encapsulation with property
    public GameState State { get; private set; }

    // Game configuration fields set in Unity Inspector
    [SerializeField] private GameObject startStation;       // Player's starting station
    [SerializeField] private Player player;                 // Reference to player GameObject

    [Tooltip("The speed time ticks during visit or transfer")]
    [SerializeField] private float tickSpeed;               // Multiplier for clock speed during visits

    [Tooltip("The start time in minutes")]
    [SerializeField] private int startTime;                 // Game start time (e.g., 9:00 AM = 540)

    [Tooltip("The end time in minutes")]
    [SerializeField] private int endTime;                   // Game end time (e.g., 5:00 PM = 1020)

    [SerializeField] private int transferTime = 5;          // Minutes required to transfer between lines

    [Tooltip("The amount to offset overlapping lines by")]
    [SerializeField] private float overlapOffset = 0.15f;   // Visual offset for overlapping train lines

    // Currently selected station (when player clicks a station)
    private Station selectedStation;

    private HashSet<Station> visitedStations;

    /// <summary>
    /// Initialize singleton and set initial game state
    /// CONSISTENCY: Good - uses Awake() for singleton initialization
    /// IMPROVEMENT: Add singleton protection (destroy if duplicate exists)
    /// </summary>
    void Awake()
    {
        Instance = this;
        State = GameState.GameStart;
        visitedStations = new HashSet<Station>();
    }

    /// <summary>
    /// Initialize game systems and start gameplay
    /// Sets player position, initializes time, and fixes overlapping line visuals
    /// IMPROVEMENT: Consider extracting initialization to separate methods
    /// </summary>
    void Start()
    {
        // Place player at starting station
        player.Initialize(startStation);

        // Transition to idle state (player can now select stations)
        State = GameState.IdleOnStation;

        // Set initial game time
        TimeUI.GameTime = startTime;

        // Fix visual overlaps for train lines that connect same stations
        OffsetOverlappingLines(overlapOffset);
    }

    /// <summary>
    /// Called when mouse enters a station's collider
    /// Shows station info and highlights available path if reachable
    /// </summary>
    /// <param name="station">The station being hovered</param>
    /// IMPROVEMENT: Extract info display logic to separate method
    /// IMPROVEMENT: Add null check for station parameter
    public void OnHoverEnterStation(Station station)
    {
        // Only respond to hover when player is idle (can select destinations)
        if (State == GameState.IdleOnStation)
        {
            // Calculate fastest path to hovered station
            TrainLine.TrainPath res = GetShortestPath(station.gameObject);

            if (res.path.Length > 0) // Station is reachable without transfer
            {
                // Highlight the station and path
                station.EnableHighlight();
                res.line.HighlightPath(res.path, res.isReverse);

                // Display station information with travel time
                InfoBoard.Instance.DisplayStationInfo(
                    station.Name,
                    station.ImageSprite,
                    res.totalTime,
                    station.VisitTime,
                    station.NearestStations,
                    InfoBoard.LineSetToString(station.Lines),
                    station.Description,
                    station.IsIntersection() ? transferTime : -1
                );
            }
            else // Unreachable without transfer, or it's the current station
            {
                // Show info but indicate unreachable (-1 travel time)
                InfoBoard.Instance.DisplayStationInfo(
                    station.Name,
                    station.ImageSprite,
                    -1,  // Unreachable marker
                    station.VisitTime,
                    station.NearestStations,
                    InfoBoard.LineSetToString(station.Lines),
                    station.Description,
                    station.IsIntersection() ? transferTime : -1
                );
            }
        }
    }

    /// <summary>
    /// Called when mouse exits a station's collider
    /// Hides station info and removes path highlighting
    /// </summary>
    /// <param name="station">The station being exited</param>
    /// IMPROVEMENT: Add null check for station parameter
    public void OnHoverExitStation(Station station)
    {
        // Only respond when player is idle
        if (State == GameState.IdleOnStation)
        {
            InfoBoard.Instance.DisableDisplay();
            station.DisableHighlight();
            DisablePathHighlight(station.Lines);
        }
    }

    /// <summary>
    /// Helper method to disable highlight on all lines in a set
    /// Used when mouse exits a station
    /// </summary>
    /// <param name="lines">Set of train lines to unhighlight</param>
    /// IMPROVEMENT: Consider moving to TrainLine as static utility
    void DisablePathHighlight(HashSet<TrainLine> lines)
    {
        foreach (TrainLine line in lines)
        {
            line.DisableHighlight();
        }
    }

    /// <summary>
    /// Deselects currently selected station and returns to idle state
    /// Called when player clicks background or cancels selection
    /// IMPROVEMENT: Fire UnityEvent when deselection occurs
    /// </summary>
    public void DeselectStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.IdleOnStation;
            VisitButtonUI.Instance.HideButton();
        }
    }

    /// <summary>
    /// Called when player clicks on a station
    /// Checks if station is reachable and shows Visit/Transfer buttons if so
    /// </summary>
    /// <param name="station">The station clicked by player</param>
    /// IMPROVEMENT: Add feedback for unreachable stations (show message)
    /// IMPROVEMENT: Add null check for station parameter
    public void SelectStation(Station station)
    {
        // Only allow selection when idle
        if (State == GameState.IdleOnStation)
        {
            var res = GetShortestPath(station.gameObject);

            // Only allow selection if station is reachable
            if (res.path.Length > 0)
            {
                State = GameState.StationSelected;
                selectedStation = station;

                // Show visit button, enable transfer button if intersection
                VisitButtonUI.Instance.DisplayButton(!visitedStations.Contains(station), station.Lines.Count > 1);
            }
        }
    }

    /// <summary>
    /// Called when player clicks Transfer button
    /// Moves player to station and ticks clock for transfer time
    /// Awards no points - used to switch train lines at intersections
    /// IMPROVEMENT: Add validation that station is actually an intersection
    /// </summary>
    public void TransferStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.Moving;

            // Move player to destination
            TraversePath(selectedStation.gameObject, () =>
            {
                // Clean up highlights
                DisablePathHighlight(selectedStation.Lines);
                selectedStation.DisableHighlight();

                // Tick clock for transfer time
                StartCoroutine(ClockTick(transferTime, () =>
                {
                    State = GameState.Arrived;
                    State = GameState.IdleOnStation;  // Immediately return to idle
                    InfoBoard.Instance.DisableDisplay();
                }));
            });

            VisitButtonUI.Instance.HideButton();
        }
    }

    /// <summary>
    /// Called when player clicks Visit button
    /// Moves player to station, awards points, updates stamp rally
    /// Checks for genre completion bonus and game end condition
    /// IMPROVEMENT: Extract stamp rally logic to separate method
    /// IMPROVEMENT: Extract game end check to separate method
    /// </summary>
    public void VisitStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.Moving;
            visitedStations.Add(selectedStation);

            // Move player to destination
            TraversePath(selectedStation.gameObject, () =>
            {
                // Clean up highlights
                DisablePathHighlight(selectedStation.Lines);
                selectedStation.DisableHighlight();

                // Tick clock for visit time
                StartCoroutine(ClockTick(selectedStation.VisitTime, () =>
                {
                    State = GameState.Arrived;

                    // Award points for visiting
                    MemoryPointUI.Points += selectedStation.Points;

                    // Activate stamp for this genre
                    StartCoroutine(StampRallyUI.GenreStamps[selectedStation.genre].ActivateNext(() =>
                    {
                        // Check if genre was completed
                        if (StampRallyUI.GenreStamps[selectedStation.genre].GenreComplete())
                        {
                            // Award completion bonus points
                            MemoryPointUI.Points += StampRallyUI.GenreStamps[selectedStation.genre].CompletionPoints;
                        }

                        // Check if time limit exceeded
                        if (CheckTime())
                        {
                            // Continue playing
                            State = GameState.IdleOnStation;
                        }
                        else
                        {
                            // Time's up - end game
                            State = GameState.GameEnd;
                            GameEndUI.OnGameEnd();
                        }

                        InfoBoard.Instance.DisableDisplay();
                    }));

                }));
            });

            VisitButtonUI.Instance.HideButton();
        }
    }

    /// <summary>
    /// Checks if game time is still within allowed range
    /// Returns false if time limit exceeded (game should end)
    /// </summary>
    /// <returns>True if time remaining, false if game should end</returns>
    /// IMPROVEMENT: More descriptive name like "HasTimeRemaining()"
    bool CheckTime()
    {
        if (TimeUI.GameTime > endTime)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Coroutine that advances game time gradually with visual feedback
    /// Used for visit time and transfer time (not travel time)
    /// </summary>
    /// <param name="timeCost">Minutes to advance</param>
    /// <param name="callback">Action to invoke when complete</param>
    /// IMPROVEMENT: Could be extracted to TimeManager class
    /// IMPROVEMENT: Add optional visual effects (progress bar, particles)
    IEnumerator ClockTick(int timeCost, Action callback)
    {
        float t = 0f;
        int prevTime = TimeUI.GameTime;

        // Gradually advance time with tickSpeed multiplier
        while (t < timeCost)
        {
            t += Time.deltaTime * tickSpeed;
            TimeUI.GameTime = prevTime + (int)t;
            yield return null;
        }

        // Ensure final time is exact
        TimeUI.GameTime = prevTime + timeCost;
        callback();
    }

    /// <summary>
    /// Finds shortest path to destination on lines shared with current station
    /// Only finds paths that don't require transfers
    /// </summary>
    /// <param name="destination">Target station GameObject</param>
    /// <returns>TrainPath with shortest route, or empty path if unreachable</returns>
    /// IMPROVEMENT: Implement A* for multi-line pathfinding with transfers
    /// IMPROVEMENT: Cache frequently used paths for performance
    TrainLine.TrainPath GetShortestPath(GameObject destination)
    {
        // Find train lines that serve both current and destination stations
        HashSet<TrainLine> setIntersect = new HashSet<TrainLine>(player.CurrentStation.GetComponent<Station>().Lines);
        setIntersect.IntersectWith(destination.GetComponent<Station>().Lines);

        // Start with unreachable path (24 hours)
        TrainLine.TrainPath path = new TrainLine.TrainPath();

        // Check each shared line and find shortest path
        foreach (TrainLine line in setIntersect)
        {
            TrainLine.TrainPath newPath = line.FindPath(player.CurrentStation, destination);
            if (newPath.totalTime < path.totalTime)
            {
                path = newPath;
            }
        }
        return path;
    }

    /// <summary>
    /// Executes player movement along path to destination
    /// Plays train sound during movement
    /// </summary>
    /// <param name="destination">Target station GameObject</param>
    /// <param name="callback">Action to invoke when movement completes</param>
    /// IMPROVEMENT: Extract to separate TravelController class
    /// IMPROVEMENT: Add cancellation support
    void TraversePath(GameObject destination, Action callback)
    {
        // Handle edge case: already at destination (transfer at same station)
        if (destination == player.CurrentStation)
        {
            callback();
            return;
        }

        // Get path and start movement
        TrainLine.TrainPath path = GetShortestPath(destination);
        AudioController.PlayTrain();

        MovePlayer(path, () =>
        {
            AudioController.StopTrain();
            callback();
        });
    }

    /// <summary>
    /// Recursively moves player through each station in path
    /// Updates time proportionally to segment travel times
    /// </summary>
    /// <param name="path">TrainPath containing route information</param>
    /// <param name="callback">Action to invoke when all segments complete</param>
    /// IMPROVEMENT: Consider iterative approach instead of recursive for better stack safety
    /// IMPROVEMENT: Add visual feedback for each segment (particles, camera follow)
    void MovePlayer(TrainLine.TrainPath path, Action callback)
    {
        GameObject[] pathArr = path.path;
        bool isReverse = path.isReverse;
        TrainLine line = path.line;
        int pathIndex = 0;

        // Nested function to handle recursive movement through path
        void NextPath()
        {
            pathIndex++;
            if (pathIndex < pathArr.Length)
            {
                // Move to next station in path
                int segmentIndex = line.DstStationToSegmentIndex(pathArr[pathIndex], isReverse);
                player.SelectDestination(pathArr[pathIndex], line.GetSegmentTime(segmentIndex), NextPath);
            }
            else
            {
                // Reached final destination
                callback();
            }
        }

        // Start movement to first station in path
        player.SelectDestination(pathArr[pathIndex], line.GetSegmentTime(line.DstStationToSegmentIndex(pathArr[0], isReverse)), NextPath);
    }

    /// <summary>
    /// Fixes visual overlap when multiple train lines connect the same two stations
    /// Offsets parallel lines perpendicular to their direction
    /// Must run after all lines are drawn (controlled by Script Execution Order in Unity)
    /// </summary>
    /// <param name="offset">Distance to offset each line</param>
    /// IMPROVEMENT: Move to separate LineRenderingUtility class
    /// IMPROVEMENT: Add editor visualization tool for adjusting offset
    /// CONSISTENCY: Good - well-documented complex algorithm
    void OffsetOverlappingLines(float offset)
    {
        // Find all LineRenderers in scene (assumes all are train line segments)
        LineRenderer[] all = FindObjectsOfType<LineRenderer>();
        Dictionary<(Vector3, Vector3), List<LineRenderer>> groups = new Dictionary<(Vector3, Vector3), List<LineRenderer>>();

        // Group all lines that connect the same two points
        foreach (var lr in all)
        {
            // Each LineRenderer should draw a single segment between two stations
            if (lr == null || lr.positionCount != 2) continue;

            Vector3 a = lr.GetPosition(0);
            Vector3 b = lr.GetPosition(1);

            // Create order-independent key (a->b and b->a are the same)
            if (a.x < b.x || (a.x == b.x && a.y < b.y)) (a, b) = (b, a);

            (Vector3, Vector3) key = (a, b);
            if (!groups.TryGetValue(key, out List<LineRenderer> list))
                groups[key] = list = new List<LineRenderer>();

            list.Add(lr);
        }

        // Process each group of overlapping lines
        foreach (var kv in groups)
        {
            List<LineRenderer> list = kv.Value;

            // Skip if no overlap (only one line)
            if (list.Count <= 1) continue;

            // Sort by material name for consistent color ordering
            list.Sort((x, y) =>
            {
                var mx = x.sharedMaterial;
                var my = y.sharedMaterial;
                return string.Compare(mx.name, my.name);
            });

            Vector3 pos1 = kv.Key.Item1;
            Vector3 pos2 = kv.Key.Item2;

            // Calculate direction of line and perpendicular offset direction
            Vector3 dir = (pos2 - pos1).normalized;
            // Cross with forward gives perpendicular in XY plane
            Vector3 side = Vector3.Cross(Vector3.forward, dir).normalized;

            // Offset lines symmetrically around center
            int n = list.Count;
            float mid = (n - 1) * 0.5f;  // Center index (e.g., 1.5 for 4 lines)

            for (int i = 0; i < n; i++)
            {
                LineRenderer data = list[i];
                // Calculate offset from center (-mid to +mid)
                float k = i - mid;
                Vector3 delta = side * (k * offset);

                // Apply perpendicular offset to both endpoints
                data.SetPosition(0, pos1 + delta);
                data.SetPosition(1, pos2 + delta);
            }
        }
    }
}
