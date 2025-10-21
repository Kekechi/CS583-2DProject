using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Station : MonoBehaviour
{
    [NonSerialized] public HashSet<TrainLine> Lines = new HashSet<TrainLine>();
    [SerializeField] private Color emission;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private AudioClip click;

    public string Name;
    public Sprite ImageSprite;
    public string Description;
    public int VisitTime;
    public int Points;
    public string NearestStations;
    public TourTokyo.Genre genre;
    private Material material;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    void Start()
    {
        label.text = Name;
    }

    public bool IsIntersection()
    {
        return Lines.Count > 1;
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
        AudioController.PlayClip(click);
        TourTokyo.Instance.SelectStation(this);
    }
}
