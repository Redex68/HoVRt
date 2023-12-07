using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class HoverBoardControllerNew : MonoBehaviour
{
    [SerializeField] List<Transform> forcePoints;
    [SerializeField] List<GameObject> fanHolders;
    Rigidbody rb;
    [SerializeField] float turnSpeed, maxUpForce, forwardForce, speedModeMaxUpForce, speedModeForwardForce, fanMinAngle, fanMaxAngle;
    [SerializeField] QuaternionVariable tiltRotation;
    [SerializeField] float topSpeed = 100f;
    [SerializeField] float minDistance = 0.5f;
    [SerializeField] float targetDistance = 3.0f;
    [SerializeField] float maxDistance = 6.0f;
    bool jumpHeld, accelerate, brake, speedMode;
    public LayerMask notPlayerLayers;

    private PlayerSoundTest sounder;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sounder = GetComponent<PlayerSoundTest>();
        rb.maxLinearVelocity = topSpeed;
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
        //if(Physics.Raycast(transform.position, Vector3.down, out hit, 100, notPlayerLayers))
        //    if(hit.distance > 4.0f && transform.eulerAngles.x < 15 || transform.eulerAngles.x > 300)
        //        transform.Rotate(Vector3.right, Time.deltaTime * 15);
        foreach (Transform forcePoint in forcePoints)
        {
            if (Physics.Raycast(forcePoint.position, -Vector3.up, out hit, 100, notPlayerLayers))
            {
                float distance;
                if (hit.distance > targetDistance)
                    distance = 1 / (1 - Mathf.InverseLerp(targetDistance, maxDistance, hit.distance));
                else
                    distance = Mathf.InverseLerp(minDistance, targetDistance, hit.distance);
                float force = rb.mass * Physics.gravity.magnitude * (speedMode ? 1f : 1/4f) / Math.Max(distance, 0.001f);
                //Debug.Log($"{forcePoint.name} : {force}");
                if (force > maxUpForce) force = maxUpForce;
                rb.AddForceAtPosition(Vector3.up * force, forcePoint.position);
            }
        }
    }

    private void HandleMotor()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100, notPlayerLayers))
        {
            Vector3 targetDirection = hit.distance < 3f ? hit.normal : Vector3.up;
            if (speedMode) rb.AddForce(Vector3.ProjectOnPlane(transform.forward, hit.normal) * speedModeForwardForce);
            else if (accelerate) rb.AddForce(Vector3.ProjectOnPlane(transform.forward, hit.normal) * forwardForce);
            else if (brake) rb.AddForce(Vector3.ProjectOnPlane(transform.forward, hit.normal) * forwardForce * -1);
            sounder.speed = Mathf.InverseLerp(0, topSpeed, rb.velocity.magnitude);
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
            transform.Rotate(Vector3.up, turnSpeed * Mathf.Pow(steerDir, 3) * Time.deltaTime);
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

    public void JumpButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) jumpHeld = true;
        if (context.phase == InputActionPhase.Canceled)
        {
            rb.AddForce(Vector3.up * 100000);
            jumpHeld = true;
        }
    }

    public void AccelerateButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) accelerate = true;
        if (context.phase == InputActionPhase.Canceled) accelerate = false;
    }

    public void BrakeButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) brake = true;
        if (context.phase == InputActionPhase.Canceled) brake = false;
    }

    public void SpeedUpButton(InputAction.CallbackContext context)
    {
        Debug.Log("HighSpeedTime");
        if (context.phase == InputActionPhase.Started)
        {
            rb.maxLinearVelocity = 5000;
            speedMode = true;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            rb.maxLinearVelocity = topSpeed;
            speedMode = false;
        }
    }
}
