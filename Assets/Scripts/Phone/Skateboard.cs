using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skateboard : MonoBehaviour
{
    [SerializeField] QuaternionVariable tiltRotation;

    // Update is called once per frame
    void Update()
    {
        if(PhoneServer.accelerometerRecent)
            transform.localRotation = tiltRotation.value;
    }
}
