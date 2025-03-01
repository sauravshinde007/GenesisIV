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
    AudioSource audioSource;
    private void Start()
    {
        if (PlayerPrefs.HasKey("QualitySetting"))
        {
            int savedQuality = PlayerPrefs.GetInt("QualitySetting");
            QualitySettings.SetQualityLevel(savedQuality);
            qualityDropdown.value = savedQuality; // Update UI dropdown
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void SetQualityLevelDropdown(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index); //Save setting
        PlayerPrefs.Save();
    }

    public void OnPlayButton()
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        cameraToggle.StartCoroutine(cameraToggle.SmoothTransition()); // Start camera transition
    }

    public void OnBackButton()
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        cameraToggle.StartCoroutine(cameraToggle.SmoothTransition()); // Move camera back
    }

    public void OnOptionButton()
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        optionPanel.SetActive(true);
    }

    public void OnOptionCloseButton()
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        optionPanel.SetActive(false);
    }

    public void OnExitButton()
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        Application.Quit();
    }

    public void LoadScene(string sceneName)
    {
        //audioManager.PlaySFX(audioManager.button);
        audioSource.Play();
        SceneManager.LoadScene(sceneName);
    }
}
