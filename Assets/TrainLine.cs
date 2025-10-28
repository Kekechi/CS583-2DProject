using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Represents a train line connecting multiple stations
/// Handles pathfinding, visual line rendering, and travel time calculations
/// IMPROVEMENT: Separate rendering logic from pathfinding logic
/// IMPROVEMENT: Consider A* pathfinding for multi-line route planning
/// CONSISTENCY: Good - uses data class (TrainPath) for complex return values
/// </summary>
public class TrainLine : MonoBehaviour
{
    /// <summary>
    /// Data class representing a path between two stations on this train line
    /// Contains the route, total time, direction, and which line to use
    /// CONSISTENCY: Good - uses data class instead of tuple or out parameters
    /// IMPROVEMENT: Could be a struct since it's just data
    /// </summary>
    public class TrainPath
    {
        /// <summary>
        /// Default constructor creates an unreachable path (24 hours travel time)
        /// Used as initial comparison value in FindPath algorithms
        /// </summary>
        public TrainPath()
        {
            path = new GameObject[0];
            totalTime = 24 * 60;  // 24 hours in minutes = unreachable
            isReverse = false;
            line = null;
        }

        /// <summary>
        /// Constructor with all path parameters
        /// </summary>
        public TrainPath(GameObject[] path, int totalTime, bool isReverse, TrainLine line)
        {
            this.path = path;
            this.totalTime = totalTime;
            this.isReverse = isReverse;
            this.line = line;
        }

        // Array of station GameObjects from source to destination (excluding source)
        public GameObject[] path;

        // Total travel time in minutes
        public int totalTime;

        // True if traveling opposite to line's defined direction
        public bool isReverse;

        // Reference to the TrainLine this path uses
        public TrainLine line;
    }

    // Ordered array of stations along this train line
    [SerializeField] private GameObject[] stations;

    // Travel time in minutes for each segment between consecutive stations
    [SerializeField] private int[] segmentTime;

    // True if this is a loop line (first and last station connect)
    [SerializeField] private bool isLoop;

    // Prefab for creating line segment visual objects
    [SerializeField] private GameObject segmentPrefab;

    // Material for the line renderer
    [SerializeField] private Material lineMaterial;

    // Emission color for highlighted path segments
    [SerializeField] private Color emission;

    // Display name of this train line (e.g., "Yamanote Line")
    public string LineName;

    // Array of LineRenderer components for each segment
    private LineRenderer[] segments;

    // Dictionary mapping station GameObject to its index in stations array
    // Allows fast O(1) lookup instead of O(n) array search
    // CONSISTENCY: Good use of dictionary for performance
    private Dictionary<GameObject, int> stationIndex = new();

    /// <summary>
    /// Initialize station index lookup dictionary
    /// CONSISTENCY: Good - uses Awake() for initialization that doesn't depend on other objects
    /// </summary>
    void Awake()
    {
        // Build fast lookup dictionary: station GameObject -> array index
        for (int i = 0; i < stations.Length; i++)
        {
            stationIndex[stations[i]] = i;
        }
    }

    /// <summary>
    /// Register this line with each station and draw visual line
    /// IMPROVEMENT: Consider moving rendering to separate method/class
    /// </summary>
    void Start()
    {
        // Add this TrainLine to each station's Lines HashSet
        for (int i = 0; i < stations.Length; i++)
        {
            stations[i].GetComponent<Station>().Lines.Add(this);
        }
        // Create visual line renderers between stations
        DrawTrainPath();
    }

    /// <summary>
    /// Empty Update method - can be removed
    /// IMPROVEMENT: Remove unused Update() to avoid unnecessary Unity overhead
    /// </summary>
    void Update()
    {

    }

    /// <summary>
    /// Creates LineRenderer GameObjects to visually connect all stations
    /// Handles both loop lines (connects back to start) and linear lines
    /// IMPROVEMENT: Consider using object pooling for segments
    /// </summary>
    void DrawTrainPath()
    {
        if (isLoop)
        {
            // Loop lines need n segments (including last->first connection)
            segments = new LineRenderer[stations.Length];
            segments[stations.Length - 1] = createSegment(stations[0].transform.position, stations[stations.Length - 1].transform.position);
        }
        else
        {
            // Linear lines need n-1 segments
            segments = new LineRenderer[stations.Length - 1];
        }

        // Create segment between each consecutive station pair
        for (int i = 0; i < stations.Length - 1; i++)
        {
            segments[i] = createSegment(stations[i].transform.position, stations[i + 1].transform.position);
        }
    }

    /// <summary>
    /// Helper method to instantiate and configure a line segment renderer
    /// </summary>
    /// <param name="pos1">Start position</param>
    /// <param name="pos2">End position</param>
    /// <returns>Configured LineRenderer component</returns>
    /// IMPROVEMENT: Consider adding line width as parameter
    LineRenderer createSegment(Vector3 pos1, Vector3 pos2)
    {
        // Instantiate segment prefab as child of this TrainLine
        GameObject obj = Instantiate(segmentPrefab, transform);
        LineRenderer renderer = obj.GetComponent<LineRenderer>();
        // Set start and end positions for the line
        renderer.SetPositions(new Vector3[] { pos1, pos2 });
        renderer.material = lineMaterial;
        return renderer;
    }

    /// <summary>
    /// Finds the fastest path between two stations on this line
    /// For loop lines, compares clockwise vs counter-clockwise routes
    /// For linear lines, determines forward vs reverse direction
    /// </summary>
    /// <param name="source">Starting station</param>
    /// <param name="destination">Destination station</param>
    /// <returns>TrainPath with fastest route</returns>
    /// IMPROVEMENT: Add validation that both stations exist on this line
    public TrainPath FindPath(GameObject source, GameObject destination)
    {
        int srcIndex = stationIndex[source];
        int dstIndex = stationIndex[destination];

        TrainPath path;

        if (isLoop)
        {
            // For loop lines, check both directions and pick faster one
            TrainPath path1 = GetPath(srcIndex, dstIndex, false);  // Clockwise
            TrainPath path2 = GetPath(srcIndex, dstIndex, true);   // Counter-clockwise
            path = path1.totalTime < path2.totalTime ? path1 : path2;
        }
        else
        {
            // For linear lines, direction is determined by source/destination order
            if (srcIndex < dstIndex)
            {
                path = GetPath(srcIndex, dstIndex, false);  // Forward direction
            }
            else
            {
                path = GetPath(srcIndex, dstIndex, true);   // Reverse direction
            }
        }

        return path;
    }

    /// <summary>
    /// Calculates path details for traveling from srcIndex to dstIndex
    /// Handles modular arithmetic for loop lines
    /// </summary>
    /// <param name="srcIndex">Source station index</param>
    /// <param name="dstIndex">Destination station index</param>
    /// <param name="isReverse">True to travel in reverse direction</param>
    /// <returns>TrainPath with stations and cumulative time</returns>
    /// IMPROVEMENT: Add comments explaining modular arithmetic
    TrainPath GetPath(int srcIndex, int dstIndex, bool isReverse)
    {
        int direction = isReverse ? -1 : 1;
        // Calculate path length using modular arithmetic (handles wrapping for loops)
        int pathLength = (stations.Length + direction * (dstIndex - srcIndex)) % stations.Length;
        int pathTime = 0;
        GameObject[] path = new GameObject[pathLength];

        // Build path array and accumulate travel time
        for (int i = 0; i < pathLength; i++)
        {
            // Calculate station index with wrapping
            int pathIndex = (stations.Length + srcIndex + direction * (i + 1)) % stations.Length;
            // Determine which segment's time to add
            int timeIndex = isReverse ? pathIndex : (srcIndex + i) % segments.Length;
            path[i] = stations[pathIndex];
            pathTime += segmentTime[timeIndex];
        }

        return new TrainPath(path, pathTime, isReverse, this);
    }

    /// <summary>
    /// Enables emission glow on line segments along the given path
    /// Used to show player's planned route
    /// </summary>
    /// <param name="path">Array of stations along the path</param>
    /// <param name="isReverse">Direction of travel</param>
    /// IMPROVEMENT: Cache emission keyword string to avoid allocations
    public void HighlightPath(GameObject[] path, bool isReverse)
    {
        foreach (GameObject station in path)
        {
            // Convert destination station to segment index
            int segmentIndex = DstStationToSegmentIndex(station, isReverse);
            segments[segmentIndex].material.EnableKeyword("_EMISSION");
            segments[segmentIndex].material.SetColor("_EmissionColor", emission);
        }
    }

    /// <summary>
    /// Converts a destination station to the index of the segment leading to it
    /// Handles reverse direction correctly
    /// </summary>
    /// <param name="station">Destination station</param>
    /// <param name="isReverse">Direction of travel</param>
    /// <returns>Index in segments array</returns>
    /// IMPROVEMENT: Add parameter validation
    public int DstStationToSegmentIndex(GameObject station, bool isReverse)
    {
        int pathIndex = stationIndex[station];
        int segmentIndex;
        if (isReverse)
        {
            // Reverse: segment is FROM this station
            segmentIndex = pathIndex;
        }
        else
        {
            // Forward: segment is TO this station (previous segment)
            segmentIndex = (pathIndex - 1 + segments.Length) % segments.Length;
        }
        return segmentIndex;
    }

    /// <summary>
    /// Disables emission glow on all line segments
    /// Called when path is no longer highlighted
    /// </summary>
    public void DisableHighlight()
    {
        foreach (LineRenderer renderer in segments)
        {
            renderer.material.DisableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Gets travel time for a specific segment by index
    /// Used during player movement animation
    /// </summary>
    /// <param name="i">Segment index</param>
    /// <returns>Travel time in minutes</returns>
    /// IMPROVEMENT: Add bounds checking
    public int GetSegmentTime(int i)
    {
        return segmentTime[i];
    }
}
