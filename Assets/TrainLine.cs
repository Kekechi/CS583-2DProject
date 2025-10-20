using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainLine : MonoBehaviour
{
    [SerializeField] private GameObject[] stations;
    [SerializeField] private int[] segmentTime;
    [SerializeField] private bool isLoop;
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color emission;
    public string LineName;
    private LineRenderer[] segments;
    private Dictionary<GameObject, int> stationIndex = new();
    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < stations.Length; i++)
        {
            stationIndex[stations[i]] = i;
        }
    }
    void Start()
    {
        for (int i = 0; i < stations.Length; i++)
        {
            stations[i].GetComponent<Station>().Lines.Add(this);
        }
        DrawTrainPath();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void DrawTrainPath()
    {

        if (isLoop)
        {
            segments = new LineRenderer[stations.Length];
            segments[stations.Length - 1] = createSegment(stations[0].transform.position, stations[stations.Length - 1].transform.position);
        }
        else
        {
            segments = new LineRenderer[stations.Length - 1];
        }

        for (int i = 0; i < stations.Length - 1; i++)
        {
            segments[i] = createSegment(stations[i].transform.position, stations[i + 1].transform.position);
        }
    }

    LineRenderer createSegment(Vector3 pos1, Vector3 pos2)
    {
        GameObject obj = Instantiate(segmentPrefab, transform);
        LineRenderer renderer = obj.GetComponent<LineRenderer>();
        renderer.SetPositions(new Vector3[] { pos1, pos2 });
        renderer.material = lineMaterial;
        return renderer;
    }

    public (GameObject[] path, int time, bool isReverse) FindPath(GameObject source, GameObject destination)
    {
        int srcIndex = stationIndex[source];
        int destIndex = stationIndex[destination];
        int pathLength = (stations.Length + destIndex - srcIndex) % stations.Length;

        int pathTime = 0;
        GameObject[] path = new GameObject[pathLength];

        for (int i = 0; i < pathLength; i++)
        {
            int pathIndex = (srcIndex + i + 1) % stations.Length;
            int timeIndex = (srcIndex + i) % segments.Length;
            path[i] = stations[pathIndex];
            pathTime += segmentTime[timeIndex];
        }

        if (isLoop)
        {
            int revPathTime = 0;
            int revPathLength = (stations.Length + srcIndex - destIndex) % stations.Length;
            GameObject[] revPath = new GameObject[revPathLength];

            for (int i = 0; i < revPathLength; i++)
            {
                int pathIndex = (stations.Length + srcIndex - i - 1) % stations.Length;
                revPath[i] = stations[pathIndex];
                revPathTime += segmentTime[pathIndex];
            }

            if (revPathTime < pathTime)
            {
                return (revPath, revPathTime, true);
            }
        }

        return (path, pathTime, false);
    }

    public void HighlightPath(GameObject[] path, bool isReverse)
    {
        foreach (GameObject station in path)
        {
            int segmentIndex = DstStationToSegmentIndex(station, isReverse);
            segments[segmentIndex].material.EnableKeyword("_EMISSION");
            segments[segmentIndex].material.SetColor("_EmissionColor", emission);
        }
    }

    public int DstStationToSegmentIndex(GameObject station, bool isReverse)
    {
        int pathIndex = stationIndex[station];
        int segmentIndex;
        if (isReverse)
        {
            segmentIndex = pathIndex;
        }
        else
        {
            segmentIndex = (pathIndex - 1 + stations.Length) % segments.Length;
        }
        return segmentIndex;
    }

    public void DisableHighlight()
    {
        foreach (LineRenderer renderer in segments)
        {
            renderer.material.DisableKeyword("_EMISSION");
        }
    }

    public int GetSegmentTime(int i)
    {
        return segmentTime[i];
    }
}
