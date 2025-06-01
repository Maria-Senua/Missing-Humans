using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform playerCamera;

    void Update()
    {
        if (playerCamera != null)
        {
            // Make the marker face the camera
            transform.LookAt(transform.position + playerCamera.forward);
        }
    }
}
