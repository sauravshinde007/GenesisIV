using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth = 5f;
    [SerializeField] private float swayMultiplier = 1f;

    private GrappleRotate grappleRotate;

    private void Start()
    {
        grappleRotate = GetComponent<GrappleRotate>(); // Get reference to GrappleRotate
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion swayRotation = rotationX * rotationY;

        // Apply sway as a local offset from the base rotation set by GrappleRotate
        transform.rotation = Quaternion.Slerp(transform.rotation, grappleRotate.GetBaseRotation() * swayRotation, Time.deltaTime * smooth);
    }
}
