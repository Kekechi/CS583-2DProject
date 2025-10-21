using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Background : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip click;
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioController.PlayClip(click);
    }
}
