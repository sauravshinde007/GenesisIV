using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector cutscene; // Drag & Drop the PlayableDirector in Inspector
    private static bool hasPlayedThisSession = false; // Resets on full game restart

    void Start()
    {
        if (hasPlayedThisSession)
        {
            gameObject.SetActive(false); // Disable Cutscene GameObject
        }
        else
        {
            cutscene.stopped += OnCutsceneEnd;
            cutscene.Play();
        }
    }

    void OnCutsceneEnd(PlayableDirector director)
    {
        hasPlayedThisSession = true; // Set it so it doesn't play again until the game restarts
        gameObject.SetActive(false); // Disable the cutscene GameObject
    }
}
