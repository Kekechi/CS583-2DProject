using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Station : MonoBehaviour
{
    [NonSerialized] public HashSet<TrainLine> Lines = new HashSet<TrainLine>();
    [SerializeField] private Color emission;

    public string Name;
    public Sprite ImageSprite;
    public string Description;
    private Material material;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    public void EnableHighlight()
    {
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
    }
    public void DisableHighlight()
    {
        material.DisableKeyword("_EMISSION");
    }

    void OnMouseEnter()
    {
        TourTokyo.Instance.OnHoverEnterStation(this);
    }

    void OnMouseExit()
    {
        TourTokyo.Instance.OnHoverExitStation(this);
    }

    void OnMouseDown()
    {
        TourTokyo.Instance.SelectStation(this);
    }
}
