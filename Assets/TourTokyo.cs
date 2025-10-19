using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    public static TourTokyo Instance { get; private set; }
    public enum GameState { GameStart, IdleOnStation, Moving, Arrived, GameEnd }
    public GameState State { get; private set; }

    [SerializeField] private TrainPath[] trainPaths;
    [SerializeField] private GameObject startStation;
    [SerializeField] private Player player;

    private TrainPath selectedLine;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player.Initialize(startStation);
    }


    public void SelectStation(GameObject station)
    {
        TraversePath(station);
    }

    void TraversePath(GameObject station)
    {
        if (station == player.CurrentStation)
        {
            return;
        }

        selectedLine = trainPaths[0];
        var res = selectedLine.FindPath(player.CurrentStation, station);
        GameObject[] path = res.path;
        float time = res.time;
        MovePlayer(path);
    }

    void MovePlayer(GameObject[] path)
    {
        int pathIndex = 0;
        void NextPath()
        {
            pathIndex++;
            if (pathIndex < path.Length)
            {
                player.SelectDestination(path[pathIndex], NextPath);
            }
            else
            {
                Debug.Log("Path Finished");
            }

        }
        player.SelectDestination(path[pathIndex], NextPath);
    }
}
