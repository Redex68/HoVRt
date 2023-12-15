using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderPush : MonoBehaviour
{
    [SerializeField] Transform center;
    [SerializeField] float maxForce;

    private Vector3 targetDir;
    private Vector3 start;
    private float depth;
    void Start()
    {
        Vector3 centerDir = center.position - transform.position;
        float v1 = Vector3.Dot(transform.right, centerDir);
        float v2 = Vector3.Dot(transform.up, centerDir);
        float v3 = Vector3.Dot(transform.forward, centerDir);

        float max = Mathf.Max(Mathf.Abs(v1), Mathf.Abs(v2), Mathf.Abs(v3));

        Vector3 size = GetComponent<BoxCollider>().size;
        Vector3 actualSize = new Vector3(size.x * transform.lossyScale.x, size.y * transform.lossyScale.y, size.z * transform.lossyScale.z);

        if(max == Mathf.Abs(v1))
        {
            targetDir = transform.right * Mathf.Sign(v1);
            depth = actualSize.x;
            start = GetComponent<BoxCollider>().ClosestPoint(center.position) - targetDir * depth;
        }
        else if(max == Mathf.Abs(v2))
        {
            targetDir = transform.up * Mathf.Sign(v2);
            depth = actualSize.y;
            start = GetComponent<BoxCollider>().ClosestPoint(center.position) - targetDir * depth;
        }
        else if(max == Mathf.Abs(v3))
        {
            targetDir = transform.forward * Mathf.Sign(v3);
            depth = actualSize.z;
            start = GetComponent<BoxCollider>().ClosestPoint(center.position) - targetDir * depth;
        }

    }

    void OnTriggerStay(Collider other)
    {
        if(other.attachedRigidbody)
        {
            Vector3 projection = Vector3.ProjectOnPlane(other.transform.position - start, targetDir) + start;
            float distance = (projection - other.transform.position).magnitude;
            float coef = 1 - Mathf.InverseLerp(depth / 4f, depth, distance);
            other.attachedRigidbody.AddForce(targetDir * maxForce * coef);
        }
    }
}
