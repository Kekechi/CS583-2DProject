using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    public enum GameState { GameStart, IdleOnStation, Moving, Arrived, GameEnd }
    public GameState State { get; private set; }

    [SerializeField] private GameObject[] trainPaths;
    [SerializeField] private GameObject startStation;
    [SerializeField] private Player player;
    // Start is called before the first frame update
    void Start()
    {
        player.Initialize(startStation);
    }


    public void SelectStation(GameObject station)
    {
        player.SelectDestination(station, () => { });
    }
}
