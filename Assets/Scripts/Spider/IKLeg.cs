using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKLeg : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float stepDistance = 4f;
    [SerializeField] float overheadOffset = 1f;
    [SerializeField] float stepHeight = 1f;
    [SerializeField] float lerpTarget = 1f;

    [SerializeField] IKLeg oppositeLeg = default;
    [SerializeField] Transform bodyReference = default;
    [SerializeField] LayerMask terrainLayer = default;
    Vector3 initialPosition, currentPosition, targetPosition, targetPointPosition, footOffset;
    Transform parentTransform;
    Vector3 rotationAxis = Vector3.up;

    float lerpValue;
    int movementDirection = 1;

    private void Start()
    {
        footOffset = transform.position - bodyReference.position;
        footOffset = new Vector3(footOffset.x, 0f, footOffset.z);

        currentPosition = targetPosition = initialPosition = transform.position;
        lerpValue = lerpTarget;
        parentTransform = transform.parent;
    }

    void Update()
    {
        transform.position = currentPosition;

        Ray ray = new Ray(bodyReference.TransformPoint(footOffset), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 10, terrainLayer))
        {
            targetPointPosition = hitInfo.point;

            if (CheckTargetPointDistance(transform.position) > stepDistance && !oppositeLeg.IsMoving() && lerpValue >= lerpTarget)
            {
                lerpValue = 0;
                movementDirection = bodyReference.InverseTransformPoint(hitInfo.point).z > bodyReference.InverseTransformPoint(targetPosition).z ? 1 : -1;
                targetPosition = hitInfo.point + bodyReference.forward * movementDirection * overheadOffset;
            }
        }

        if (lerpValue < lerpTarget)
        {
            Vector3 tempPosition = Vector3.Lerp(initialPosition, targetPosition, lerpValue);
            tempPosition.y += Mathf.Sin(lerpValue * Mathf.PI) * stepHeight;
            currentPosition = tempPosition;
            lerpValue += Time.deltaTime * moveSpeed;
        }
        else
        {
            initialPosition = targetPosition;
        }
    }

    private float CheckTargetPointDistance(Vector3 point)
    {
        Debug.DrawLine(point, targetPointPosition, Color.red);
        return Vector3.Distance(targetPointPosition, point);
    }

    public bool IsMoving()
    {
        return lerpValue < lerpTarget;
    }
}
