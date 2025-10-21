using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    public static TourTokyo Instance { get; private set; }
    public enum GameState { GameStart, IdleOnStation, StationSelected, Moving, Arrived, GameEnd }
    public enum Genre { Landmark, Culture, Museum, Nature, Shopping };
    public GameState State { get; private set; }
    [SerializeField] private GameObject startStation;
    [SerializeField] private Player player;
    [SerializeField] private float tickSpeed;
    [SerializeField] private int startTime;
    [SerializeField] private int endTime;
    [SerializeField] private int transferTime = 5;

    private Station selectedStation;

    void Awake()
    {
        Instance = this;
        State = GameState.GameStart;
    }

    // Start is called before the first frame update
    void Start()
    {
        player.Initialize(startStation);
        State = GameState.IdleOnStation;
        TimeUI.Instance.GameTime = startTime;
        OffsetOverlappingLines(0.1f);
        InfoBoard.Instance.DisableDisplay();
    }

    public void OnHoverEnterStation(Station station)
    {
        if (State == GameState.IdleOnStation)
        {
            TrainLine.TrainPath res = GetShortestPath(station.gameObject);
            if (res.path.Length > 0)
            {
                station.EnableHighlight();
                res.line.HighlightPath(res.path, res.isReverse);
                InfoBoard.Instance.DisplayStationInfo(station.Name, station.ImageSprite, res.totalTime, station.VisitTime, InfoBoard.LineSetToString(station.Lines), station.Description, station.IsIntersection() ? transferTime : -1);
            }
            else
            {
                InfoBoard.Instance.DisplayStationInfo(station.Name, station.ImageSprite, -1, station.VisitTime, InfoBoard.LineSetToString(station.Lines), station.Description, station.IsIntersection() ? transferTime : -1);
            }
        }
    }
    public void OnHoverExitStation(Station station)
    {
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
                    MemoryPointUI.Instance.Points += selectedStation.Points;

                    StartCoroutine(StampRallyUI.GenreStamps[selectedStation.genre].ActivateNext(() =>
                    {
                        if (StampRallyUI.GenreStamps[selectedStation.genre].GenreComplete())
                        {
                            MemoryPointUI.Instance.Points += StampRallyUI.GenreStamps[selectedStation.genre].CompletionPoints;
                        }

                        if (CheckTime())
                        {
                            State = GameState.IdleOnStation;
                        }
                        else
                        {
                            State = GameState.GameEnd;
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
        if (TimeUI.Instance.GameTime > endTime)
        {
            return false;
        }
        return true;
    }

    IEnumerator ClockTick(int timeCost, Action callback)
    {
        float t = 0f;
        int prevTime = TimeUI.Instance.GameTime;
        while (t < timeCost)
        {
            t += Time.deltaTime * tickSpeed;
            TimeUI.Instance.GameTime = prevTime + (int)t;
            yield return null;
        }

        TimeUI.Instance.GameTime = prevTime + timeCost;
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
            return;
        }

        TrainLine.TrainPath path = GetShortestPath(destination);
        MovePlayer(path, callback);
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
    void OffsetOverlappingLines(float offset)
    {
        var all = FindObjectsOfType<LineRenderer>();
        var groups = new Dictionary<(Vector3, Vector3), List<LineRenderer>>();

        foreach (var lr in all)
        {
            if (lr == null || lr.positionCount != 2) continue;

            Vector3 a = lr.GetPosition(0);
            Vector3 b = lr.GetPosition(1);

            // Order-independent tuple
            if (a.x < b.x || (a.x == b.x && a.y < b.y)) (a, b) = (b, a);

            var key = (a, b);
            if (!groups.TryGetValue(key, out var list))
                groups[key] = list = new List<LineRenderer>();

            list.Add(lr);
        }

        foreach (var kv in groups)
        {
            var list = kv.Value;
            if (list.Count <= 1) continue;

            // Sort by material (guaranteed unique per overlap)
            list.Sort((x, y) =>
            {
                var mx = x.sharedMaterial;
                var my = y.sharedMaterial;
                return string.Compare(mx.name, my.name);
            });


            Vector3 pos1 = kv.Key.Item1;
            Vector3 pos2 = kv.Key.Item2;

            // Compute direction/side once
            Vector3 dir = (pos2 - pos1).normalized;
            Vector3 side = Vector3.Cross(Vector3.forward, dir).normalized;

            int n = list.Count;
            float mid = (n - 1) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                var data = list[i];
                float k = i - mid;
                Vector3 delta = side * (k * offset);

                data.SetPosition(0, pos1 + delta);
                data.SetPosition(1, pos2 + delta);
            }
        }
    }
}
