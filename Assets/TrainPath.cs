using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainPath : MonoBehaviour
{
    public GameObject[] stations;
    public bool isLoop;
    private LineRenderer line;
  // Start is called before the first frame update
  void Awake()
  {
        line = GetComponent<LineRenderer>();
  }
  void Start()
    {
        drawTrainPath();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void drawTrainPath()
    {
        line.loop = isLoop;
        line.positionCount = stations.Length;
        
        for( int i = 0; i < stations.Length; i++)
        {
            line.SetPosition(i, stations[i].transform.position);
        }
    }
}
