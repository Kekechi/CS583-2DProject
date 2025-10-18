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

    [SerializeField] private GameObject destStation;

    public void Initialize(GameObject startStation)
    {
        CurrentStation = startStation;
        transform.position = startStation.transform.position + playerStationOffset;
        SelectDestination(destStation, () => { });
    }

    public void SelectDestination(GameObject station, Action onComplete)
    {
        isMoving = true;
        StartCoroutine(AnimateMovement(station, onComplete));
    }

    IEnumerator AnimateMovement(GameObject targetStation, Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetStation.transform.position + playerStationOffset;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        isMoving = false;
        CurrentStation = targetStation;
        transform.position = targetPos;
        onComplete?.Invoke();
    }
}
