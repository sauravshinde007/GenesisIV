using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject Door;
    public bool doorIsOpening;
    [SerializeField] private float doorOpenLoc;
    [SerializeField] private float doorOpenSpeed = 2f; // Adjust for slower/faster opening

    private Vector3 initialPosition;
    private Vector3 targetPosition;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Start()
    {
        initialPosition = Door.transform.position;
        targetPosition = new Vector3(initialPosition.x, doorOpenLoc, initialPosition.z);
    }

    void Update()
    {
        if (doorIsOpening)
        {
            Door.transform.position = Vector3.Lerp(Door.transform.position, targetPosition, Time.deltaTime * doorOpenSpeed);

            if (Vector3.Distance(Door.transform.position, targetPosition) < 0.01f)
            {
                doorIsOpening = false; // Stop opening when near the target
            }
        }
    }
    void OnMouseDown()
    { //THIS FUNCTION WILL DETECT THE MOUSE CLICK ON A COLLIDER,IN OUR CASE WILL DETECT THE CLICK ON THE BUTTON

        doorIsOpening = true;
        audioManager.PlaySFX(audioManager.button);
        //if we click on the button door we must start to open
    }
}
