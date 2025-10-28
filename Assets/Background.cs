using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles click interactions on the background UI element
/// Implements IPointerClickHandler for Unity's Event System
/// IMPROVEMENT: Consider using UnityEvent to decouple from AudioController
/// IMPROVEMENT: Could extend to handle other pointer events (hover, drag, etc.)
/// </summary>
public class Background : MonoBehaviour, IPointerClickHandler
{
    // Audio clip to play when background is clicked
    [SerializeField] private AudioClip click;

    /// <summary>
    /// Unity Event System callback when pointer clicks on this UI element
    /// Plays click sound through AudioController
    /// IMPROVEMENT: Check if click is null before playing
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioController.PlayClip(click);
    }
}
