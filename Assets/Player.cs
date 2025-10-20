using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject CurrentStation { get; private set; }
    private bool isMoving;
    [SerializeField] private Vector3 playerStationOffset;
    [SerializeField] private float duration = 1f;

    public void Initialize(GameObject startStation)
    {
        CurrentStation = startStation;
        transform.position = startStation.transform.position + playerStationOffset;
    }

    public void SelectDestination(GameObject station, int timeCost, Action onComplete)
    {
        isMoving = true;
        StartCoroutine(AnimateMovement(station, timeCost, onComplete));
    }

    IEnumerator AnimateMovement(GameObject targetStation, int timeCost, Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetStation.transform.position + playerStationOffset;
        float t = 0f;
        int prevTime = TimeUI.Instance.GameTime;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            TimeUI.Instance.GameTime = prevTime + (int)(t * timeCost);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        TimeUI.Instance.GameTime = prevTime + timeCost;
        isMoving = false;
        CurrentStation = targetStation;
        transform.position = targetPos;
        onComplete?.Invoke();
    }
}
