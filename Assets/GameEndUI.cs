using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI score;
    public static GameEndUI Instance;
    void Start()
    {
        gameObject.SetActive(false);
        Instance = this;
    }

    public static void OnGameEnd()
    {
        Instance.gameObject.SetActive(true);
        Instance.score.text = $"Memory Points: {MemoryPointUI.Points}";
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
