using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainPath : MonoBehaviour
{
    [SerializeField] private GameObject[] stations;
    [SerializeField] private float[] segmentTime;
    [SerializeField] private bool isLoop;
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
        }
    }

    public GameObject[] FindPath(GameObject source, GameObject destination)
    {
        int srcIndex = stationIndex[source];
        int destIndex = stationIndex[destination];
        int pathLength = destIndex - srcIndex;

        float pathTime = 0;
        GameObject[] path = new GameObject[pathLength];

        for (int i = 0; i < pathLength; i++)
        {
            path[i] = stations[srcIndex + i + 1];
            pathTime += segmentTime[srcIndex + i];
        }

        if (isLoop)
        {
            float revPathTime = 0;
            int revPathLength = stations.Length + srcIndex - destIndex;
            GameObject[] revPath = new GameObject[revPathLength];

        }

        return path;

    }
}
