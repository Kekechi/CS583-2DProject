using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameObject currentStation;
    private bool isMoving;
    private Vector3 targetPosition;
    public Vector3 playerStationOffset;
    public float speed;

    public void initialize(GameObject startStation)
    {
        currentStation = startStation;
        transform.position = startStation.transform.position + playerStationOffset;
    }
}
