using System.Collections;
using UnityEngine;

public class BossArena : MonoBehaviour
{
    public BossAI boss;
    public GameObject door; // Assign the door GameObject in Inspector
    public float doorCloseHeight = -3f; // How much to lower the door when closing
    public float doorSpeed = 2f; // Speed of door closing
    public GameObject levelChanger; // Assign the LevelChanger object in Inspector

    private Vector3 doorInitialPosition;
    private bool doorClosed = false;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss == null) return;

            boss.isPlayerInBossArea = true;
            boss.healthBar.gameObject.SetActive(true);

            if (!doorClosed && door != null)
            {
                StartCoroutine(CloseDoor());
            }
        }
    }

    IEnumerator CloseDoor()
    {
        doorClosed = true;
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
