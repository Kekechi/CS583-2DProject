using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the stamp rally collection system UI
/// Tracks completion of station visits by genre and awards bonus points
/// IMPROVEMENT: Consider using ScriptableObject for genre stamp data
/// IMPROVEMENT: Use events to notify when genres are completed
/// CONSISTENCY: Good - uses dictionary for fast genre lookup
/// </summary>
public class StampRallyUI : MonoBehaviour
{
    /// <summary>
    /// Represents a stamp collection for a specific genre (Landmark, Culture, etc.)
    /// Handles visual animation and completion tracking for stamps
    /// IMPROVEMENT: Could be separated into its own file
    /// IMPROVEMENT: Add validation that Group contains correct number of images
    /// </summary>
    [System.Serializable]
    public class GenreStamp
    {
        // The genre category this stamp collection represents
        public TourTokyo.Genre Genre;

        // Sprite for active/collected stamps
        public Sprite Icon;

        // Sprite for inactive/uncollected stamps
        public Sprite Gray;

        // Parent GameObject containing all stamp image slots
        public GameObject Group;

        // Bonus points awarded when all stamps in genre are collected
        public int CompletionPoints = 100;

        // Array of stamp slot images (cached from Group's children)
        private Image[] images;

        // Current activation index (tracks how many stamps are collected)
        private int i = 0;

        // Audio clip to play when activating a stamp
        private AudioClip clip;

        /// <summary>
        /// Initialize stamp collection by caching child images and setting to gray
        /// Called once at Start() by parent StampRallyUI
        /// </summary>
        /// <param name="clip">Audio clip to play when stamp is activated</param>
        /// IMPROVEMENT: Add null checks for Group and clip
        public void Initialize(AudioClip clip)
        {
            this.clip = clip;
            // Get all Image components in children, excluding the parent Group GameObject
            images = Group.GetComponentsInChildren<Image>().Where(c => c.gameObject != Group).ToArray();
            // Set all stamps to inactive (gray) initially
            foreach (Image img in images) { img.sprite = Gray; }
        }

        /// <summary>
        /// Coroutine that animates the next stamp activation with scale/rotation effect
        /// Changes sprite from gray to icon with visual feedback
        /// </summary>
        /// <param name="callback">Action to invoke after animation completes</param>
        /// IMPROVEMENT: Use AnimationCurve for smoother scaling
        /// IMPROVEMENT: Consider DOTween for more complex animations
        public IEnumerator ActivateNext(Action callback)
        {
            // Only activate if there are uncollected stamps remaining
            if (!GenreComplete())
            {
                // Store original scale to restore after animation
                Vector3 origScale = images[i].transform.localScale;

                // Animation parameters
                float t = 0f;                 // Interpolation parameter
                float startScale = 2f;        // Stamp starts at 2x size
                float finalScale = 1f;        // Ends at normal size
                float startRot = 90f;         // Rotation parameters (currently unused)
                float finalRot = 0f;

                // Change sprite from gray to colored icon
                images[i].sprite = Icon;

                // Animate scale down from 2x to 1x over time
                while (t < 1f)
                {
                    t += Time.deltaTime * 2f;  // Animation speed multiplier
                    // Lerp scale from startScale to finalScale
                    images[i].transform.localScale = origScale * (startScale + t * (finalScale - startScale));
                    yield return null;  // Wait one frame
                }

                // Ensure final scale is exact
                images[i].transform.localScale = origScale;

                // Play stamp collection sound
                AudioController.PlayClip(clip);

                // Move to next stamp slot
                i++;
            }

            // Notify completion
            callback();
        }

        /// <summary>
        /// Returns the number of stamps activated in this genre
        /// IMPROVEMENT: Return i directly (i already represents count)
        /// </summary>
        public int ActivatedCount()
        {
            return i + 1;
        }

        /// <summary>
        /// Checks if all stamps in this genre have been collected
        /// Used to determine if bonus points should be awarded
        /// </summary>
        public bool GenreComplete()
        {
            return i == images.Length;
        }
    }

    // Array of all genre stamp collections (configured in Inspector)
    [SerializeField] private GenreStamp[] stamps;

    // Sound to play when collecting a stamp
    [SerializeField] private AudioClip stampSound;

    // Static dictionary for fast genre lookup from anywhere
    // IMPROVEMENT: Consider instance-based approach with singleton instead of static dictionary
    public static Dictionary<TourTokyo.Genre, GenreStamp> GenreStamps = new Dictionary<TourTokyo.Genre, GenreStamp>();

    /// <summary>
    /// Initialize all genre stamps and populate lookup dictionary
    /// IMPROVEMENT: Change to Awake() to ensure dictionary is populated before other scripts' Start()
    /// IMPROVEMENT: Validate that all TourTokyo.Genre enum values have corresponding stamps
    /// </summary>
    void Start()
    {
        // Initialize each stamp collection and add to dictionary for fast lookup
        foreach (GenreStamp stamp in stamps)
        {
            stamp.Initialize(stampSound);
            GenreStamps[stamp.Genre] = stamp;
        }
    }

}
