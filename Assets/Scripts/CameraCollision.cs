using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public Transform target;        // Reference to the player
    public float distance = 4.0f;   // Default distance from player
    public float minDistance = 1.0f; // Minimum allowed distance
    public float maxDistance = 4.0f; // Maximum allowed distance
    public float smoothSpeed = 10.0f; // Smooth transition speed
    public LayerMask collisionMask; // Walls and obstacles layer

    private Vector3 dollyDirection; // Original direction between camera and player
    private float currentDistance;

    void Start()
    {
        dollyDirection = transform.localPosition.normalized;
        currentDistance = distance;
    }

    void LateUpdate()
    {
        Vector3 desiredCameraPosition = target.position + (target.rotation * dollyDirection * maxDistance);
        RaycastHit hit;

        // Check for collision
        if (Physics.Linecast(target.position, desiredCameraPosition, out hit, collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance * 0.9f, minDistance, maxDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, maxDistance, Time.deltaTime * smoothSpeed);
        }

        transform.localPosition = dollyDirection * currentDistance;
    }
}
