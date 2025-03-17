using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class BossArena : MonoBehaviour
{
    public BossAI boss;
    public GameObject door; // Assign the door GameObject in Inspector
    public float doorCloseHeight = -3f; // How much to lower the door when closing
    public float doorSpeed = 2f; // Speed of door closing
    public GameObject levelChanger; // Assign the LevelChanger object in Inspector
    public PlayableDirector cutscene; // Assign the cutscene PlayableDirector in Inspector

    private Vector3 doorInitialPosition;
    private bool doorClosed = false;
    private bool cutscenePlayed = false;

    AudioManager audioManager;

    private void Start()
    {
        if (levelChanger != null)
        {
            levelChanger.SetActive(false); // Hide LevelChanger initially
        }

        if (door != null)
        {
            doorInitialPosition = door.transform.position; // Store original door position
        }

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss == null) return;

            boss.isPlayerInBossArea = true;
            boss.healthBar.gameObject.SetActive(true);

            if (!cutscenePlayed && cutscene != null)
            {
                PlayCutscene();
            }

            if (!doorClosed && door != null)
            {
                StartCoroutine(CloseDoor());
            }
        }
    }

    private void PlayCutscene()
    {
        cutscenePlayed = true;

        if (boss != null)
        {
            boss.enabled = false; // Disable boss behavior during cutscene
        }
        cutscene.Play();
        StartCoroutine(WaitForCutsceneToEnd());
    }

    IEnumerator WaitForCutsceneToEnd()
    {
        while (cutscene.state == PlayState.Playing)
        {
            yield return null;
        }

        // Enable boss behavior after cutscene ends
        if (boss != null)
        {
            boss.enabled = true;
        }
    }

    IEnumerator CloseDoor()
    {
        doorClosed = true;
        audioManager.PlaySFX(audioManager.doorClose);
        Vector3 targetPosition = new Vector3(doorInitialPosition.x, doorInitialPosition.y + doorCloseHeight, doorInitialPosition.z);

        while (Vector3.Distance(door.transform.position, targetPosition) > 0.01f)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, targetPosition, Time.deltaTime * doorSpeed);
            yield return null;
        }

        door.transform.position = targetPosition;
    }

    public void BossDefeated()
    {
        if (levelChanger != null)
        {
            levelChanger.SetActive(true); // Activate LevelChanger when boss dies
        }
    }
}
