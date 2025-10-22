using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class InfoBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image infoImage;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI visitTime;
    [SerializeField] private TextMeshProUGUI lines;
    [SerializeField] private TextMeshProUGUI description;

    [SerializeField] private TextMeshProUGUI stations;
    public static InfoBoard Instance;

    void Awake()
    {
        Instance = this;
        DisableDisplay();
    }


    public void DisplayStationInfo(string name, Sprite image, int timeCost, int visitTime, string stationNames, string lineNames, string newDescription, int transferTime = -1)
    {
        nameText.text = name;
        infoImage.sprite = image;
        stations.text = $"Nearest Stations: {stationNames}";
        lines.text = $"Train Lines: {lineNames}";
        time.text = timeCost == -1 ? "Unreachable without Transfer" : $"Estimated Time: <b>{timeCost} min</b>";
        this.visitTime.text = $"Visit Time: <b>{visitTime} min</b>{((transferTime == -1) ? "" : $"  Transfer Time: <b>{transferTime} min</b>")}";
        description.text = newDescription;
        gameObject.SetActive(true);
    }

    public void DisableDisplay()
    {
        gameObject.SetActive(false);
    }

    public static string LineSetToString(HashSet<TrainLine> lines)
    {
        return String.Join(", ", lines.Select(x => x.LineName));
    }
}
