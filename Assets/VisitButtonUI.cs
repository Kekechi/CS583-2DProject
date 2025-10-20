using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VisitButtonUI : MonoBehaviour
{
    public static VisitButtonUI Instance;
    [SerializeField] private Button visit;
    [SerializeField] private Button transfer;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void DisplayButton(bool isIntersection)
    {
        gameObject.SetActive(true);
        transfer.interactable = isIntersection;
    }

    public void HideButton()
    {
        gameObject.SetActive(false);
    }
}
