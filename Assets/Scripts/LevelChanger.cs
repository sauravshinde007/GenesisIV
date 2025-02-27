using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private bool isScaling = false;
    private bool canTransition = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isScaling)
        {
            StartCoroutine(ScaleObject());
        }
    }

    IEnumerator ScaleObject()
    {
        isScaling = true;

        float duration = 1f; // Time for each scaling step

        // Step 1: Scale Z from 0 to 0.2
        yield return StartCoroutine(ScaleOverTime(transform,
            new Vector3(transform.localScale.x, transform.localScale.y, 0),
            new Vector3(transform.localScale.x, transform.localScale.y, 0.2f), duration));

        // Step 2: Scale X from 0 to 0.3
        yield return StartCoroutine(ScaleOverTime(transform,
            new Vector3(0, transform.localScale.y, transform.localScale.z),
            new Vector3(0.3f, transform.localScale.y, transform.localScale.z), duration));

        canTransition = true; // Now player can collide and load the scene
    }

    IEnumerator ScaleOverTime(Transform objTransform, Vector3 startScale, Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            objTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
            yield return null;
        }
        objTransform.localScale = targetScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canTransition && collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
