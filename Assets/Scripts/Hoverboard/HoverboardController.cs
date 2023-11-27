using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel, rightWheel;
    [NonSerialized] public Vector3 leftOrigin, rightOrigin;
    public MotorType motorType; // is this wheel attached to motor?
    public enum MotorType
    {
        Motor,
        None
    }
    public TurnType turnType;
    public enum TurnType
    {
        None,
        Normal,
        Inverted,
    }
    AxleInfo(WheelCollider leftWheel, WheelCollider rightWheel, Transform leftTransform, Transform rightTransform, MotorType motorType, TurnType turnType) { 
        this.leftWheel = leftWheel;
        this.rightWheel = rightWheel;
        this.motorType = motorType; 
        this.turnType = turnType;
    }
}

public class HoverboardController : MonoBehaviour
{
    [SerializeField] public List<AxleInfo> axleInfos;
    [NonSerialized] public int groundedWheels = 0;
    [NonSerialized] public bool grappling;
    public float gravCheckDistance;
    [SerializeField] Transform centerOfMass;
    [NonSerialized] public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false, respawn2D;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    [SerializeField] internal Transform[] roadCheckers;
    [SerializeField] internal QuaternionVariable tiltRotation;
    //CineMachine3DController camController;
    private float currentBreakForce;
    private float currentSteerAngle;
    public int playerIndex = 0;
    bool breaking;
    [NonSerialized] public Vector3 startPoint;
    [NonSerialized] public Quaternion? startRot = null;
    internal float stationaryTolerance;
    internal Rigidbody rigidBody; // rigid body of the car
    public Vector3? localCustomGravity = null;
    [NonSerialized] public Vector2 rotateDir, moveDir, swingDir;
    public LayerMask gravRoadLayer;
    public LayerMask notCarLayers;
    //PlayerInput playerInput;
    [NonSerialized] public float gravRoadPercent;
    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;
    public Vector3? customGravity = null;
    private Camera mainCamera;

    void OnEnable()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (centerOfMass != null) rigidBody.centerOfMass = centerOfMass.position;

        stationaryTolerance = 0.001f;
        //playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        startPoint = transform.position;
        startRot = transform.rotation;
    }

    public void SetCam(GameObject cameras, int playNum)
    {
        playerIndex = playNum;
        mainCamera = cameras.GetComponentInChildren<Camera>();
        //camController = cameras.GetComponentInChildren<CineMachine3DController>();
    }

    private void OnDisable() { }

    private void FixedUpdate()
    {
        //if (camController == null) return;
        if (respawned)
        {
            RespawnLock();
            return;
        }
        CheckGrounded();
        HandleMotor();
        if (groundedWheels != 0) HandleSteering();
        AirRotate();
        CustomGravity();
        UpdateTimers();
    }

    void UpdateTimers()
    {
        //Updates Timers
        //jumpTimer -= Time.deltaTime;
        respawnTimer -= Time.deltaTime;
    }

    void RespawnLock()
    {
        //Makes sure that the momentum of the car is removed on a respawn
        if (respawned)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                respawned = false;
                rigidBody.isKinematic = false;
            }
            else return;
        }
    }

    private void HandleMotor()
    {
        float driveDir = 1;
        float rpm = 0;
        int motorAmount = 0;

        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.motorType == AxleInfo.MotorType.Motor)
            {
                axle.leftWheel.motorTorque = driveDir * motorForce;
                axle.rightWheel.motorTorque = driveDir * motorForce;
                rpm += axle.leftWheel.rpm + axle.rightWheel.rpm;
                motorAmount++;
            }
        }
        rpm /= motorAmount;
        currentBreakForce = (breaking || (driveDir > 0 && rpm < -1) || (driveDir < 0 && rpm > 1)) ? breakForce : (driveDir == 0 ? breakForce / 10 : 0);
        ApplyBreaking();
    }

    private void HandleSteering()
    {
        if (PhoneServer.accelerometerRecent)
        {
            float steerDir = 0;
            if (tiltRotation.value.x < -0.05) steerDir = 1;
            else if (tiltRotation.value.x > 0.05) steerDir = -1;
            else steerDir = -tiltRotation.value.x / 0.05f;

            currentSteerAngle = maxSteeringAngle * steerDir;
            foreach (AxleInfo axle in axleInfos)
            {
                if (axle.turnType == AxleInfo.TurnType.Normal)
                {
                    axle.leftWheel.steerAngle = currentSteerAngle;
                    axle.rightWheel.steerAngle = currentSteerAngle;
                }
                else if (axle.turnType == AxleInfo.TurnType.Inverted)
                {
                    axle.leftWheel.steerAngle = -currentSteerAngle;
                    axle.rightWheel.steerAngle = -currentSteerAngle;
                }
            }
        }
    }

    private void ApplyBreaking()
    {
        foreach (AxleInfo axle in axleInfos)
        {
            axle.leftWheel.brakeTorque = currentBreakForce;
            axle.rightWheel.brakeTorque = currentBreakForce;
        }
    }

    public void SetLocalCustomGravity(Vector3? gravityDirection)
    {
        localCustomGravity = gravityDirection;
    }

    internal void CustomGravity()
    {
        bool noGravChanges = true;
        CheckGravRoad();
        if (gravRoadPercent > 0.7)
        {
            rigidBody.AddForce(Physics.gravity.magnitude * rigidBody.mass * -transform.up);
            noGravChanges = false;
        }
        else if (localCustomGravity != null)
        {
            noGravChanges = false;
            rigidBody.AddForce((Vector3)localCustomGravity * rigidBody.mass);
        }
        else if (customGravity != null)
        {
            noGravChanges = false;
            rigidBody.AddForce((Vector3)customGravity * rigidBody.mass);
        }
        if (noGravChanges) rigidBody.useGravity = true;
        else rigidBody.useGravity = false;
    }

    internal void CheckGravRoad()
    {
        gravRoadPercent = 0;
        int notGravRoadAmount = 0;
        RaycastHit hit;
        foreach (Transform roadChecker in roadCheckers)
        {
            if (Physics.Raycast(roadChecker.position, -roadChecker.transform.up, out hit, gravCheckDistance, notCarLayers) && (gravRoadLayer == (gravRoadLayer | (1 << hit.transform.gameObject.layer))))
            {
                gravRoadPercent++;
            }
            else if (Physics.Raycast(roadChecker.position, -roadChecker.transform.up, out hit, gravCheckDistance, notCarLayers) && (gravRoadLayer == (gravRoadLayer | (1 << hit.transform.gameObject.layer))))
            {

            }
            else notGravRoadAmount++;
        }
        gravRoadPercent /= gravRoadPercent + notGravRoadAmount;
    }

    internal void CheckGrounded()
    {
        groundedWheels = 0;
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.leftWheel.isGrounded) groundedWheels++;
            if (axle.rightWheel.isGrounded) groundedWheels++;

        }
    }

    /*public void Reset(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) Respawn();
    }*/

    public void Kill()
    {
        //moveSound.mute = true;
        //moveSound.pitch = 0;
        //carSound.PlayOneShot(deadSoundclip, 0.5f);
        Respawn();
    }

    internal void Respawn()
    {
        if (Time.timeScale == 0.0f) return;
        if (lastCheckPoint == null)
        {
            rigidBody.MovePosition(startPoint);
            if (startRot != null) rigidBody.MoveRotation((Quaternion)startRot);
        }
        else
        {
            rigidBody.MovePosition(lastCheckPoint.gameObject.transform.position);
            rigidBody.MoveRotation(lastCheckPoint.gameObject.transform.rotation);
        }
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        foreach (AxleInfo aInfo in axleInfos)
        {
            aInfo.leftWheel.brakeTorque = float.MaxValue;
            aInfo.rightWheel.brakeTorque = float.MaxValue;

            aInfo.leftWheel.motorTorque = 0;
            aInfo.rightWheel.motorTorque = 0;
        }
        respawnTimer = 0.1f;
        respawned = true;
        grappling = false;
    }

    private void AirRotate()
    {
        if (groundedWheels != 0 || grappling) return;

        //Front rotate
        //rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * rotateDir.y);
    }
}

