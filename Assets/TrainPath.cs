using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainPath : MonoBehaviour
{
    [SerializeField] private GameObject[] stations;
    [SerializeField] private float[] segmentTime;
    [SerializeField] private bool isLoop;
    public string LineName;
    private LineRenderer line;
    private Dictionary<GameObject, int> stationIndex = new();
    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();

        for (int i = 0; i < stations.Length; i++)
        {
            stationIndex[stations[i]] = i;
        }
    }
    void Start()
    {
        DrawTrainPath();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void DrawTrainPath()
    {
        line.loop = isLoop;
        line.positionCount = stations.Length;

        for (int i = 0; i < stations.Length; i++)
        {
            line.SetPosition(i, stations[i].transform.position);

            stations[i].GetComponent<Station>().Lines.Add(this);
        }
    }

    public (GameObject[] path, float time) FindPath(GameObject source, GameObject destination)
    {
        int srcIndex = stationIndex[source];
        int destIndex = stationIndex[destination];
        int pathLength = (stations.Length + destIndex - srcIndex) % stations.Length;

        float pathTime = 0;
        GameObject[] path = new GameObject[pathLength];

        for (int i = 0; i < pathLength; i++)
        {
            int pathIndex = (srcIndex + i + 1) % stations.Length;
            int timeIndex = (srcIndex + i) % stations.Length;
            path[i] = stations[pathIndex];
            pathTime += segmentTime[timeIndex];
        }

        if (isLoop)
        {
            float revPathTime = 0;
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
                return (revPath, revPathTime);
            }
        }

        return (path, pathTime);
    }
}
