using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCollisionRotation : MonoBehaviour
{
    public LayerMask collisionLayers; // Layers that will trigger rotation
    public float rotationAngle = 90f; // Angle to rotate when colliding
    private Quaternion originalRotation; // Store initial rotation

    private void Start()
    {
        originalRotation = transform.rotation; // Store the original gun rotation
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, collisionLayers))
        {
            RotateGun();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, collisionLayers))
        {
            ResetGunRotation();
        }
    }

    void RotateGun()
    {
        transform.rotation = Quaternion.Euler(rotationAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        Debug.Log("Rotating gun");
    }

    void ResetGunRotation()
    {
        transform.rotation = originalRotation;
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) > 0;
    }
}
