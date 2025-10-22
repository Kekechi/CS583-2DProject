using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    // static Instance variable for easy outside access of the script
    // Only works because there is only one instance of TourTokyo
    public static TourTokyo Instance { get; private set; }

    // Possible GameState in game
    public enum GameState { GameStart, IdleOnStation, StationSelected, Moving, Arrived, GameEnd }
    // Possible genre of any stop / station
    public enum Genre { Landmark, Culture, Museum, Nature, Shopping };
    public GameState State { get; private set; }
    [SerializeField] private GameObject startStation;
    [SerializeField] private Player player;

    [Tooltip("The speed time ticks during visit or transfer")]
    [SerializeField] private float tickSpeed;
    [Tooltip("The start time in minutes")]
    [SerializeField] private int startTime;
    [Tooltip("The end time in minutes")]
    [SerializeField] private int endTime;
    [SerializeField] private int transferTime = 5;
    [Tooltip("The amount to offset overlapping lines by")]
    [SerializeField] private float overlapOffset = 0.15f;

    private Station selectedStation;

    void Awake()
    {
        Instance = this;
        State = GameState.GameStart;
    }

    void Start()
    {
        player.Initialize(startStation);
        State = GameState.IdleOnStation;
        TimeUI.GameTime = startTime;

        OffsetOverlappingLines(overlapOffset);
    }

    public void OnHoverEnterStation(Station station)
    {
        // Only when player is choosing next station
        if (State == GameState.IdleOnStation)
        {
            // Get path to the hovered station from the current player position
            TrainLine.TrainPath res = GetShortestPath(station.gameObject);
            if (res.path.Length > 0) // If the station is reachable without transfer
            {
                station.EnableHighlight(); // Highlight hovering station
                res.line.HighlightPath(res.path, res.isReverse); // Highlight path to the hovering station
                // Display hovering station information
                InfoBoard.Instance.DisplayStationInfo(station.Name, station.ImageSprite, res.totalTime, station.VisitTime, station.NearestStations, InfoBoard.LineSetToString(station.Lines), station.Description, station.IsIntersection() ? transferTime : -1);
            }
            else // Unreachable without transfer, or hovering at the current station
            {
                InfoBoard.Instance.DisplayStationInfo(station.Name, station.ImageSprite, -1, station.VisitTime, station.NearestStations, InfoBoard.LineSetToString(station.Lines), station.Description, station.IsIntersection() ? transferTime : -1);
            }
        }
    }
    public void OnHoverExitStation(Station station)
    {
        // Only when player is choosing next station
        if (State == GameState.IdleOnStation)
        {
            InfoBoard.Instance.DisableDisplay();
            station.DisableHighlight();
            DisablePathHighlight(station.Lines);
        }
    }

    void DisablePathHighlight(HashSet<TrainLine> lines)
    {
        foreach (TrainLine line in lines)
        {
            line.DisableHighlight();
        }
    }

    public void DeselectStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.IdleOnStation;
            VisitButtonUI.Instance.HideButton();
        }
    }

    public void SelectStation(Station station)
    {
        // User is allowed to select a station only when gamestate is IdleOnStation
        if (State == GameState.IdleOnStation)
        {
            var res = GetShortestPath(station.gameObject);
            if (res.path.Length > 0)
            {
                State = GameState.StationSelected;
                selectedStation = station;
                VisitButtonUI.Instance.DisplayButton(station.Lines.Count > 1);
            }
        }
    }

    public void TransferStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.Moving;
            TraversePath(selectedStation.gameObject, () =>
            {
                DisablePathHighlight(selectedStation.Lines);
                selectedStation.DisableHighlight();
                StartCoroutine(ClockTick(transferTime, () =>
                {
                    State = GameState.Arrived;
                    State = GameState.IdleOnStation;
                    InfoBoard.Instance.DisableDisplay();
                }));
            });
            VisitButtonUI.Instance.HideButton();
        }
    }
    public void VisitStation()
    {
        if (State == GameState.StationSelected)
        {
            State = GameState.Moving;
            TraversePath(selectedStation.gameObject, () =>
            {
                DisablePathHighlight(selectedStation.Lines);
                selectedStation.DisableHighlight();
                StartCoroutine(ClockTick(selectedStation.VisitTime, () =>
                {
                    State = GameState.Arrived;
                    MemoryPointUI.Points += selectedStation.Points;

                    StartCoroutine(StampRallyUI.GenreStamps[selectedStation.genre].ActivateNext(() =>
                    {
                        if (StampRallyUI.GenreStamps[selectedStation.genre].GenreComplete())
                        {
                            MemoryPointUI.Points += StampRallyUI.GenreStamps[selectedStation.genre].CompletionPoints;
                        }

                        if (CheckTime())
                        {
                            State = GameState.IdleOnStation;
                        }
                        else
                        {
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

    bool CheckTime()
    {
        if (TimeUI.GameTime > endTime)
        {
            return false;
        }
        return true;
    }

    IEnumerator ClockTick(int timeCost, Action callback)
    {
        float t = 0f;
        int prevTime = TimeUI.GameTime;
        while (t < timeCost)
        {
            t += Time.deltaTime * tickSpeed;
            TimeUI.GameTime = prevTime + (int)t;
            yield return null;
        }

        TimeUI.GameTime = prevTime + timeCost;
        callback();
    }

    TrainLine.TrainPath GetShortestPath(GameObject destination)
    {
        HashSet<TrainLine> setIntersect = new HashSet<TrainLine>(player.CurrentStation.GetComponent<Station>().Lines);
        setIntersect.IntersectWith(destination.GetComponent<Station>().Lines);
        TrainLine.TrainPath path = new TrainLine.TrainPath();

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
    void TraversePath(GameObject destination, Action callback)
    {
        if (destination == player.CurrentStation)
        {
            callback();
            return;
        }

        TrainLine.TrainPath path = GetShortestPath(destination);
        AudioController.PlayTrain();
        MovePlayer(path, () =>
        {
            AudioController.StopTrain();
            callback();
        });
    }

    void MovePlayer(TrainLine.TrainPath path, Action callback)
    {
        GameObject[] pathArr = path.path;
        bool isReverse = path.isReverse;
        TrainLine line = path.line;
        int pathIndex = 0;
        void NextPath()
        {
            pathIndex++;
            if (pathIndex < pathArr.Length)
            {
                int segmentIndex = line.DstStationToSegmentIndex(pathArr[pathIndex], isReverse);
                player.SelectDestination(pathArr[pathIndex], line.GetSegmentTime(segmentIndex), NextPath);
            }
            else
            {
                callback();
            }
        }
        player.SelectDestination(pathArr[pathIndex], line.GetSegmentTime(line.DstStationToSegmentIndex(pathArr[0], isReverse)), NextPath);
    }


    // Fix lines that goes through same two station
    // Must happen after every lines are drawn (Controlled through execution order is setting)
    void OffsetOverlappingLines(float offset)
    {
        // Find all line renderer in scene, this assumes that every line renderer is a train line
        LineRenderer[] all = FindObjectsOfType<LineRenderer>();
        Dictionary<(Vector3, Vector3), List<LineRenderer>> groups = new Dictionary<(Vector3, Vector3), List<LineRenderer>>();

        // Group all line that draws between same two line
        foreach (var lr in all)
        {
            // Each LineRenderer should be drawing a single line between two stations
            if (lr == null || lr.positionCount != 2) continue;

            Vector3 a = lr.GetPosition(0);
            Vector3 b = lr.GetPosition(1);

            // Order-independent tuple
            if (a.x < b.x || (a.x == b.x && a.y < b.y)) (a, b) = (b, a);

            (Vector3, Vector3) key = (a, b);
            if (!groups.TryGetValue(key, out List<LineRenderer> list))
                groups[key] = list = new List<LineRenderer>();

            list.Add(lr);
        }


        foreach (var kv in groups)
        {
            List<LineRenderer> list = kv.Value;
            // If there is only one line at a position, no need to offset
            if (list.Count <= 1) continue;

            // Sort by material, the order of line colors become consistent
            list.Sort((x, y) =>
            {
                var mx = x.sharedMaterial;
                var my = y.sharedMaterial;
                return string.Compare(mx.name, my.name);
            });


            Vector3 pos1 = kv.Key.Item1;
            Vector3 pos2 = kv.Key.Item2;

            // Direction of the line
            Vector3 dir = (pos2 - pos1).normalized;
            // The perpendicular direction in the forward plane
            Vector3 side = Vector3.Cross(Vector3.forward, dir).normalized;

            // Offset lines about the middle
            int n = list.Count;
            float mid = (n - 1) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                LineRenderer data = list[i];
                float k = i - mid;
                Vector3 delta = side * (k * offset);

                data.SetPosition(0, pos1 + delta);
                data.SetPosition(1, pos2 + delta);
            }
        }
    }
}
