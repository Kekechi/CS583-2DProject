using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourTokyo : MonoBehaviour
{
    public GameObject[] trainPaths;
    public GameObject startStation;
    public Player player;
    // Start is called before the first frame update
    void Start()
    {
        player.initialize(startStation);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
