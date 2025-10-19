using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Station : MonoBehaviour
{
    [NonSerialized] public HashSet<TrainPath> Lines = new HashSet<TrainPath>();
    public string Name;
    public Sprite ImageSprite;
    public string Description;

    void OnMouseEnter()
    {

        InfoBoard.Instance.DisplayStationInfo(Name, ImageSprite, 2, InfoBoard.LineSetToString(Lines), Description);
    }

    void OnMouseExit()
    {
        InfoBoard.Instance.DisableDisplay();
    }
    void OnMouseDown()
    {
        TourTokyo.Instance.SelectStation(gameObject);
    }
}
