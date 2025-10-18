using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    private TourTokyo game;

    void Start()
    {
        game = Camera.main.GetComponent<TourTokyo>();
    }
    void OnMouseEnter()
    {

    }

    void OnMouseExit()
    {

    }
    void OnMouseDown()
    {
        game.SelectStation(gameObject);
    }
}
