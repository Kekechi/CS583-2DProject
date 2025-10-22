using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject CurrentStation { get; private set; }
    [SerializeField] private Vector3 playerStationOffset;
    [SerializeField] private float duration = 1f;

    // Set position to startStation, set CurrentStation 
    public void Initialize(GameObject startStation)
    {
        CurrentStation = startStation;
        transform.position = startStation.transform.position + playerStationOffset;
    }

    // Start Player movement to destination station (Linearly)
    // call onComplete after player reaches the destination
    public void SelectDestination(GameObject station, int timeCost, Action onComplete)
    {
        StartCoroutine(AnimateMovement(station, timeCost, onComplete));
    }

    // The animation function to run as coroutine to move player to destination over time
    IEnumerator AnimateMovement(GameObject targetStation, int timeCost, Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetStation.transform.position + playerStationOffset;
        float t = 0f;
        int prevTime = TimeUI.GameTime;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            TimeUI.GameTime = prevTime + (int)(t * timeCost);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        TimeUI.GameTime = prevTime + timeCost;
        CurrentStation = targetStation;
        transform.position = targetPos;
        onComplete?.Invoke();
    }
}
