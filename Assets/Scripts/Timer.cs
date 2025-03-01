using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Timer : MonoBehaviour
{
    private float timeElapsed = 0f; // Start from 0 and count up
    private bool timerRunning = true;
    public TextMeshProUGUI timerText; // Assign in Inspector

    [Header("Scenes Without Timer")]
    [SerializeField] private string[] scenesWithoutTimer; // Add scene names in Inspector

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Listen for scene changes
    }

    private void Start()
    {
        CheckSceneAndToggleTimer(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime; // Count up
            UpdateTimerDisplay();
        }
    }

    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndToggleTimer(scene.name);
    }

    private void CheckSceneAndToggleTimer(string sceneName)
    {
        bool shouldHideTimer = System.Array.Exists(scenesWithoutTimer, scene => scene == sceneName);

        if (shouldHideTimer)
        {
            timerText.gameObject.SetActive(false); // Hide Timer UI
            timerRunning = false; // Pause Timer
        }
        else
        {
            timerText.gameObject.SetActive(true); // Show Timer UI
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timeElapsed = 0f; // Reset to 0
        timerRunning = true;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(timeElapsed);
            int milliseconds = Mathf.FloorToInt((timeElapsed - seconds) * 100);
            timerText.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to prevent memory leaks
    }
}
