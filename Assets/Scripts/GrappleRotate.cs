using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRotate : MonoBehaviour
{
    public Gun grapple;
    private Quaternion desiredRotation;
    void Update()
    {
        if(!grapple.isSwinging()){ 
           desiredRotation = transform.parent.rotation;
        }else{
            desiredRotation = Quaternion.LookRotation(grapple.GetSwingPoint() - transform.position);

        }
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * 10f);
    }
}

