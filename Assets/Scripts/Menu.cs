using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Camera Reference")]
    public CameraToggle cameraToggle; // Drag the CameraToggle script in Inspector
    public GameObject optionPanel;
    [SerializeField] private Dropdown qualityDropdown;

    public void SetQualityLevelDropdown(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void OnPlayButton()
    {
        cameraToggle.StartCoroutine(cameraToggle.SmoothTransition()); // Start camera transition
    }

    public void OnBackButton()
    {
        cameraToggle.StartCoroutine(cameraToggle.SmoothTransition()); // Move camera back
    }

    public void OnOptionButton()
    {
        optionPanel.SetActive(true);
    }

    public void OnOptionCloseButton()
    {
        optionPanel.SetActive(false);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
