using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum CalibrationEvent {LEFT, RIGHT, DOWN, FINISHED}

public class Tilt : MonoBehaviour
{
    [Tooltip("How long you have to turn tilt the device in a specific direction before the script starts reading the accelerometer data")]
    [SerializeField] public float readDelay {get; private set;} = 1.0f;
    [Tooltip("How long the script will read the data (longer == better average)")]
    [SerializeField] public float readLength {get; private set;} = 2.0f;
    [SerializeField] GameEvent calibrationEvent;

    public static Quaternion tiltRotation = Quaternion.identity;

    private Quaternion turn = Quaternion.identity;

    // Update is called once per frame
    void Update()
    {
        if(PhoneServer.accelerometerRecent)
        {
            Vector3 translated = GetTranslated();
            tiltRotation = Quaternion.FromToRotation(Vector3.down, turn * translated);
        }
    }

    public void Calibrate()
    {
        StartCoroutine(CalibrateCoroutine());
    }


    private IEnumerator CalibrateCoroutine()
    {
        Debug.Log("Reading left tilt");
        calibrationEvent.Raise(this, CalibrationEvent.LEFT);
        StartCoroutine(GetTilt());
        yield return new WaitUntil(() => __tiltReturn != Vector3.zero);
        Vector3 left = __tiltReturn;
        
        Debug.Log("Reading right tilt");
        calibrationEvent.Raise(this, CalibrationEvent.RIGHT);
        StartCoroutine(GetTilt());
        yield return new WaitUntil(() => __tiltReturn != Vector3.zero);
        Vector3 right = __tiltReturn;
        
        Debug.Log("Reading down direction");
        calibrationEvent.Raise(this, CalibrationEvent.DOWN);
        StartCoroutine(GetTilt());
        yield return new WaitUntil(() => __tiltReturn != Vector3.zero);
        Vector3 down = __tiltReturn;


        Vector3 forward = Vector3.Cross(right, left);
        down = Vector3.ProjectOnPlane(down, forward);

        turn = Quaternion.FromToRotation(forward, Vector3.right);
        down = turn * down;
        turn = Quaternion.FromToRotation(down, Vector3.down) * turn;
        calibrationEvent.Raise(this, CalibrationEvent.FINISHED);
        Debug.Log("Calibrated");
    }

/// <summary> Reads the phone's accelerometer data and averages it out over time </summary>
    private Vector3 __tiltReturn = Vector3.zero;
    private IEnumerator GetTilt()
    {
        __tiltReturn = Vector3.zero;
        Vector3 tilt = Vector3.zero;

        yield return new WaitForSeconds(readDelay);
        float start = Time.time;

        while(Time.time - start < readLength)
        {
            tilt += GetTranslated() * Time.deltaTime;
            yield return new WaitForNextFrameUnit();
        }
        __tiltReturn = tilt;
    }

/// <summary> Maps the phone's axes to the ingame ones <summary>
    private Vector3 GetTranslated()
    {
        return new Vector3(-PhoneServer.accelerometerData.y, PhoneServer.accelerometerData.z, PhoneServer.accelerometerData.x).normalized;
    }
}