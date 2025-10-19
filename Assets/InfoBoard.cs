using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoard : MonoBehaviour
{
    private Text nameText;
    private Image infoImage;
    private Text time;
    private Text lines;
    private Text description;
    public static InfoBoard Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        nameText = transform.Find("Name").GetComponent<Text>();
        infoImage = transform.Find("Image").GetComponent<Image>();
        time = transform.Find("Time").GetComponent<Text>();
        lines = transform.Find("Line").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();
    }

    public void DisplayStationInfo(string name, Sprite image, int timeCost, string lineNames, string newDescription)
    {

        nameText.text = name;
        infoImage.sprite = image;
        lines.text = lineNames;
        time.text = $"Estimated Time: {timeCost} min";
        description.text = newDescription;
        gameObject.SetActive(true);
    }

    public void DisableDisplay()
    {
        gameObject.SetActive(false);
    }

    public static string LineSetToString(HashSet<TrainPath> lines)
    {
        string lineStr = "";
        foreach (TrainPath l in lines)
        {
            lineStr += l.LineName + " ";
        }
        return lineStr;
    }
}
