using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class HoverBoardControllerNew : MonoBehaviour
{
    [SerializeField] List<Transform> forcePoints;
    [SerializeField] List<GameObject> fanHolders;
    Rigidbody rb;
    [SerializeField] float turnSpeed, upForce, maxUpForce, forwardForce, fanMinAngle, fanMaxAngle;
    [SerializeField] QuaternionVariable tiltRotation;
    public LayerMask notPlayerLayers;

    private PlayerSoundTest sounder;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sounder = GetComponent<PlayerSoundTest>();
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleSteering();
        Hover();
        HandleMotor();
        HandleFanHolders();
    }

    internal void Hover()
    {
        RaycastHit hit;
        foreach (Transform forcePoint in forcePoints)
        {
            if (Physics.Raycast(forcePoint.position, -Vector3.up, out hit, 100, notPlayerLayers))
            {
                float force = upForce / (hit.distance > 0.001f ? hit.distance : 0.001f);
                if (force > maxUpForce) force = maxUpForce;
                rb.AddForceAtPosition(Vector3.up * force, forcePoint.position);
                //Debug.DrawLine(forcePoint.position, forcePoint.position + Vector3.up*force / 10);
            }
        }
    }

    private void HandleMotor()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100, notPlayerLayers))
        {
            rb.AddForce(Vector3.ProjectOnPlane(transform.forward, hit.normal) * forwardForce);
			float speed = Mathf.InverseLerp(0, 50, rb.velocity.magnitude);
            //Debug.Log(speed);
            sounder.speed = speed;
        }
    }

    private void HandleSteering()
    {
        if (PhoneServer.accelerometerRecent)
        {
            float steerDir = 0;
            if (tiltRotation.value.x < -0.07) steerDir = 1;
            else if (tiltRotation.value.x > 0.07) steerDir = -1;
            else if (Math.Abs(tiltRotation.value.x) > 0.02) steerDir = -tiltRotation.value.x / 0.07f;
            //Debug.Log(tiltRotation.value.x);
            transform.Rotate(transform.up, turnSpeed * Mathf.Pow(steerDir, 3) * Time.deltaTime);
            //transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y + 0.6f * steerDir * Time.deltaTime, transform.localRotation.z, transform.localRotation.w);
            //rb.AddTorque(rb.transform.up * turnForce * steerDir);
        }
    }

    private void HandleFanHolders()
    {
        float angle = Mathf.Lerp(fanMinAngle, fanMaxAngle, Mathf.InverseLerp(0, 50, Vector3.Project(rb.velocity, transform.forward).magnitude));
        foreach (GameObject fanHolder in fanHolders)
        {
            fanHolder.transform.localRotation = new Quaternion(0, 0, 0, 0);
            fanHolder.transform.Rotate(Vector3.up * angle);
        }
    }
}
