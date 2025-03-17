using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRotate : MonoBehaviour
{
    public Gun grapple;
    private Quaternion desiredRotation;
    private Quaternion baseRotation; // Store base rotation for sway effect

    void Update()
    {
        if (!grapple.isSwinging())
        {
            desiredRotation = transform.parent.rotation;
        }
        else
        {
            desiredRotation = Quaternion.LookRotation(grapple.GetSwingPoint() - transform.position);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * 10f);

        // Store base rotation for WeaponSway
        baseRotation = transform.rotation;
    }

    public Quaternion GetBaseRotation()
    {
        return baseRotation;
    }
}
