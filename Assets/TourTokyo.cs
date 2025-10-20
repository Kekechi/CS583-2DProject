using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    public static TourTokyo Instance { get; private set; }
    public enum GameState { GameStart, IdleOnStation, StationSelected, Moving, Arrived, GameEnd }
    public GameState State { get; private set; }
    [SerializeField] private GameObject startStation;
    [SerializeField] private Player player;
    [SerializeField] private float tickSpeed;
    [SerializeField] private int startTime;
    [SerializeField] private int endTime;
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
    }

    public void OnHoverEnterStation(Station station)
    {
        if (State == GameState.IdleOnStation)
        {
            InfoBoard.Instance.DisplayStationInfo(station.Name, station.ImageSprite, 2, InfoBoard.LineSetToString(station.Lines), station.Description);

            var res = GetShortestPath(station.gameObject);
            if (res.path.Length > 0)
            {
                station.EnableHighlight();
                res.line.HighlightPath(res.path, res.isReverse);
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
                StartCoroutine(ClockTick(5, () =>
                {
                    State = GameState.Arrived;
                    State = GameState.IdleOnStation;
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
                StartCoroutine(ClockTick(60, () =>
                {
                    State = GameState.Arrived;
                    if (CheckTime())
                    {
                        State = GameState.IdleOnStation;
                    }
                    else
                    {
                        State = GameState.GameEnd;
                    }
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

    (GameObject[] path, TrainLine line, bool isReverse) GetShortestPath(GameObject destination)
    {
        HashSet<TrainLine> setIntersect = new HashSet<TrainLine>(player.CurrentStation.GetComponent<Station>().Lines);
        setIntersect.IntersectWith(destination.GetComponent<Station>().Lines);
        GameObject[] path = new GameObject[0];
        int time = 24 * 60;
        TrainLine selectedLine = null;
        bool isReverse = false;

        foreach (TrainLine line in setIntersect)
        {
            var res = line.FindPath(player.CurrentStation, destination);
            if (res.time < time)
            {
                time = res.time;
                selectedLine = line;
                isReverse = res.isReverse;
                path = res.path;
            }
        }
        return (path, selectedLine, isReverse);
    }
    void TraversePath(GameObject destination, Action callback)
    {
        if (destination == player.CurrentStation)
        {
            return;
        }

        var res = GetShortestPath(destination);
        MovePlayer(res.path, res.line, res.isReverse, callback);
    }

    void MovePlayer(GameObject[] path, TrainLine line, bool isReverse, Action callback)
    {
        int pathIndex = 0;
        void NextPath()
        {
            pathIndex++;
            if (pathIndex < path.Length)
            {
                int segmentIndex = line.DstStationToSegmentIndex(path[pathIndex], isReverse);
                player.SelectDestination(path[pathIndex], line.GetSegmentTime(segmentIndex), NextPath);
            }
            else
            {
                callback();
            }
        }
        player.SelectDestination(path[pathIndex], line.GetSegmentTime(line.DstStationToSegmentIndex(path[0], isReverse)), NextPath);
    }
}
