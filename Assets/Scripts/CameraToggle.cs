using UnityEngine;
using System.Collections;

public class CameraToggle : MonoBehaviour
{
    private Vector3 positionA = new Vector3(9.15f, 1.25f, 3.5f);
    private Quaternion rotationA = new Quaternion(0.0f, -0.7071057558059692f, 0.0f, 0.7071079015731812f);
    
    private Vector3 positionB = new Vector3(3.9f, 1.5f, 3.5f);
    private Quaternion rotationB = new Quaternion(-0.4999985098838806f, -0.4999999403953552f, -0.5000000596046448f, 0.5000014901161194f);
    
    private bool isAtPositionA = true;
    public float transitionSpeed = 2.0f;
    private Coroutine moveCoroutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            moveCoroutine = StartCoroutine(SmoothTransition());
        }
    }

    public IEnumerator SmoothTransition()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 targetPosition = isAtPositionA ? positionB : positionA;
        Quaternion targetRotation = isAtPositionA ? rotationB : rotationA;
        
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            yield return null;
        }
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        isAtPositionA = !isAtPositionA;
    }
}
