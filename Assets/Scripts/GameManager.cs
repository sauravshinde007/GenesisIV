using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private string[] uiScenes; // Scenes where cursor should be enabled
    [SerializeField] private GameObject optionPanel; // Assign this in Inspector

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateCursorState(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCursorState(scene.name);
    }

    private void UpdateCursorState(string sceneName)
    {
        bool isUIScene = System.Array.Exists(uiScenes, scene => scene == sceneName);

        if (isUIScene)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (!isPaused) // Ensure cursor remains hidden when not paused
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ToggleOptions()
    {
        isPaused = !isPaused;

        if (optionPanel != null)
        {
            optionPanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f; // Pause or Resume Game
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void MenuButton()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(false); // Disable Option Panel before switching scenes
        }

        Time.timeScale = 1f; // Resume time before switching scene
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
