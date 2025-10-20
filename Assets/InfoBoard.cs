using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image infoImage;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI lines;
    [SerializeField] private TextMeshProUGUI description;
    public static InfoBoard Instance;

    void Awake()
    {
        Instance = this;
    }


    public void DisplayStationInfo(string name, Sprite image, int timeCost, string lineNames, string newDescription)
    {
        nameText.text = name;
        infoImage.sprite = image;
        lines.text = $"Train Lines: {lineNames}";
        time.text = $"Estimated Time: <b>{timeCost} min</b>";
        description.text = newDescription;
        gameObject.SetActive(true);
    }

    public void DisableDisplay()
    {
        gameObject.SetActive(false);
    }

    public static string LineSetToString(HashSet<TrainLine> lines)
    {
        string lineStr = "";
        foreach (TrainLine l in lines)
        {
            lineStr += l.LineName + " ";
        }
        return lineStr;
    }
}
