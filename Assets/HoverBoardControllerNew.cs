using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HoverBoardControllerNew : MonoBehaviour
{
    [SerializeField] List<Transform> forcePoints;
    Rigidbody rb;
    [SerializeField] float turnSpeed, upForce, maxUpForce, forwardForce;
    public LayerMask notPlayerLayers;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleSteering();
        Hover();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100, notPlayerLayers))
        {
            rb.AddForce(Vector3.ProjectOnPlane(transform.forward, hit.normal) * forwardForce);
        }
    }

    internal void Hover()
    {
        RaycastHit hit;
        foreach (Transform forcePoint in forcePoints)
        {
            Debug.Log("raycasting");
            if (Physics.Raycast(forcePoint.position, -Vector3.up, out hit, 100, notPlayerLayers))
            {
                Debug.Log("Hit! Distance is " + hit.distance);
                float force = upForce / (hit.distance > 0.001f ? hit.distance : 0.001f);
                Debug.Log("Force: " + force);
                if (force > maxUpForce) force = maxUpForce;
                rb.AddForceAtPosition(Vector3.up * force, forcePoint.position);
                Debug.DrawLine(forcePoint.position, forcePoint.position + Vector3.up*force / 10);
            }
        }
    }

    private void HandleSteering()
    {
        if (PhoneServer.accelerometerRecent)
        {
            float steerDir = 0;
            if (Tilt.tiltRotation.x < -0.07) steerDir = 1;
            else if (Tilt.tiltRotation.x > 0.07) steerDir = -1;
            else if (Math.Abs(Tilt.tiltRotation.x) > 0.02) steerDir = -Tilt.tiltRotation.x / 0.07f;
            rb.AddTorque(Vector3.up * turnSpeed * steerDir);
            //transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y + 0.6f * steerDir * Time.deltaTime, transform.localRotation.z, transform.localRotation.w);
            //rb.AddTorque(rb.transform.up * turnForce * steerDir);
        }
    }
}
