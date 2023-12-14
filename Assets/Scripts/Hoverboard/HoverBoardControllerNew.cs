using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.InputSystem;

public class HoverBoardControllerNew : MonoBehaviour
{
    [SerializeField] List<Transform> forcePoints;
    [SerializeField] List<GameObject> fanHolders;
    Rigidbody rb;
    [SerializeField] float turnSpeed, maxUpForce, forwardForce, speedModeForwardForce, fanMinAngle, fanMaxAngle;
    [SerializeField] QuaternionVariable tiltRotation;
    [SerializeField] float topSpeed = 50f;
    [SerializeField] float topSpeedModeSpeed = 100f;
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
        //if(Physics.Raycast(transform.position, Vector3.down, out hit, 100, notPlayerLayers))
        //    if(hit.distance > 4.0f && transform.eulerAngles.x < 15 || transform.eulerAngles.x > 300)
        //        transform.Rotate(Vector3.right, Time.deltaTime * 15);
        RaycastHit[] hits = new RaycastHit[4];
        float minHeight = forcePoints[0].position.y;
        float maxHeight = forcePoints[0].position.y;
        float[] modifiers = {0f, 0f, 0f, 0f};
        for(int i = 0; i < forcePoints.Count; i++)
        {
            Transform forcePoint = forcePoints[i];
            if (!Physics.Raycast(forcePoint.position + Vector3.up, Vector3.down, out hits[i], 100, notPlayerLayers))
            {
                hits[i].distance = -1f;
            }
            hits[i].distance -= 1f;
            if(minHeight > forcePoint.position.y) minHeight = forcePoint.position.y;
            else if(maxHeight < forcePoint.position.y) maxHeight = forcePoint.position.y;
        }

        for(int i = 0; i < forcePoints.Count; i++)
        {
            modifiers[i] = (minHeight == maxHeight) ? 1 : -(Mathf.InverseLerp(minHeight, maxHeight, forcePoints[i].position.y) - 0.5f) * 2;
            modifiers[i] /= Mathf.Lerp(10, 100, 1 - Mathf.InverseLerp(0, 90, Vector3.Angle(Vector3.up, transform.up)));
        }
        
        //Check if we're too close to the ground
        if(hits.Any((hit) => 
        {
            float downSpeed = Vector3.Dot(-hit.normal, rb.velocity);
            return hit.distance < minDistance && downSpeed > 0.5;
        }))
        {
            //If we're too close, take the closest force point and push the hover board away in the direction of the ground's normal at that point.
            float distance = hits[0].distance;
            float distanceSum = 0.0f;
            Vector3 normal = hits[0].normal;
            float[] modifiers2 = new float[hits.Length];
            for(int i = 0; i < hits.Length; i++)
            {
                if(distance > hits[i].distance)
                {
                    distance = hits[i].distance;
                    normal = hits[i].normal;
                }
                modifiers2[i] = 1.0f / hits[i].distance;
                distanceSum += modifiers2[i];
            }
            distanceSum /= modifiers2.Length;
            for(int i = 0; i < hits.Length; i++)
            {
                modifiers2[i] /= distanceSum;
            }
            
            float downSpeed = Vector3.Dot(-normal, rb.velocity) / Mathf.Lerp(4, 24, Mathf.InverseLerp(minDistance * 0.5f, minDistance, distance));
            Normalize(ref modifiers2, 0.1f, 0.025f);

            for(int i = 0; i < forcePoints.Count; i++)
            {
                rb.AddForceAtPosition(transform.up * downSpeed * modifiers2[i], forcePoints[i].position, ForceMode.VelocityChange);
            }
        }
        else
        {
            float[] forces = new float[forcePoints.Count];
            for(int i = 0; i < forcePoints.Count; i++)
            {
                float modifier;
                if(hits[i].distance > maxDistance)
                {
                    modifier = modifiers[i];
                }
                else if(hits[i].distance == -2f) continue;
                else if(hits[i].distance > targetDistance)
                    modifier = 1 - Mathf.InverseLerp(targetDistance, maxDistance, hits[i].distance);
                else
                    modifier = Mathf.Lerp(1, maxUpForce / rb.mass / Physics.gravity.magnitude, 1 - Mathf.InverseLerp(minDistance, targetDistance, hits[i].distance));
                forces[i] = rb.mass * Physics.gravity.magnitude / forcePoints.Count * modifier;
            }
            Normalize(ref forces, 25f, 5f);
            for(int i = 0; i < forcePoints.Count; i++)
            {
                rb.AddForceAtPosition(transform.up * forces[i], forcePoints[i].position);
            }
        }
    }

    private void HandleMotor()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100, notPlayerLayers))
        {
            float actualTopSpeed = speedMode ? topSpeedModeSpeed : topSpeed;
            float coef = 1 - Mathf.InverseLerp(actualTopSpeed * 0.8f, actualTopSpeed, Vector3.Project(rb.velocity, transform.forward).magnitude);

            Vector3 projectionNormal = hit.distance < 3f ? hit.normal : Vector3.up;
            Vector3 targetDirection = Vector3.ProjectOnPlane(transform.forward, projectionNormal);
            
            if (speedMode) rb.AddForce(targetDirection * speedModeForwardForce * coef);
            else if (accelerate) rb.AddForce(targetDirection * forwardForce * coef);
            else if (brake) rb.AddForce(targetDirection * forwardForce * -1);
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

    private void Normalize(ref float[] values, float maxDelta, float step)
    {
        float average = values.Average();
        
        for(int j = 0; j < 2000 && (Mathf.Max(values) - Mathf.Min(values) > maxDelta); j++)
        {
            float[] modifiers = new float[values.Length];
            for(int i = 0; i < values.Length; i++)
                modifiers[i] = average - values[i];
            
            float normFactor = Mathf.Max(Mathf.Abs(Mathf.Min(modifiers)), Mathf.Max(modifiers));

            for(int i = 0; i < values.Length; i++)
            {
                values[i] += modifiers[i] / normFactor * step;
            }
        }
    }

    public void JumpButton(InputAction.CallbackContext context)
    {
        /*if (context.phase == InputActionPhase.Started) jumpHeld = true;
        if (context.phase == InputActionPhase.Canceled)
        {
            rb.AddForce(Vector3.up * 100000);
            jumpHeld = true;
        }*/
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
            speedMode = true;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            speedMode = false;
        }
    }
}
