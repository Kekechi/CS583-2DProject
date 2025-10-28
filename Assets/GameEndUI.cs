using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game-over screen UI that displays final score
/// Uses singleton pattern for easy access from other systems
/// IMPROVEMENT: Use Awake() instead of Start() to ensure Instance is set before other scripts need it
/// IMPROVEMENT: Add null check for singleton pattern (check if Instance already exists)
/// IMPROVEMENT: Use UnityEvent to notify listeners of game end instead of static method
/// </summary>
public class GameEndUI : MonoBehaviour
{
    // Text component to display final memory points score
    [SerializeField] private TextMeshProUGUI score;

    // Singleton instance for global access
    // IMPROVEMENT: Should be private with public property for encapsulation
    public static GameEndUI Instance;

    /// <summary>
    /// Initialize singleton and hide UI at start
    /// IMPROVEMENT: Change to Awake() to avoid race conditions with other Start() methods
    /// </summary>
    void Start()
    {
        gameObject.SetActive(false);
        Instance = this;
    }

    /// <summary>
    /// Static method to trigger game end display
    /// Shows UI panel and updates score text with final memory points
    /// IMPROVEMENT: Add null check for Instance before accessing
    /// IMPROVEMENT: Consider using events instead of static method call
    /// </summary>
    public static void OnGameEnd()
    {
        Instance.gameObject.SetActive(true);
        Instance.score.text = $"Memory Points: {MemoryPointUI.Points}";
    }

    /// <summary>
    /// Restarts the current scene (called by Restart button)
    /// Resets all game state by reloading the scene
    /// IMPROVEMENT: Consider using a GameManager to handle scene transitions
    /// IMPROVEMENT: Add fade transition for smoother user experience
    /// </summary>
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
