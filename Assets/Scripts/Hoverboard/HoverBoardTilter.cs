using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverBoardTilter : MonoBehaviour
{
    [SerializeField] QuaternionVariable tiltRotation;

    // Update is called once per frame
    void Update()
    {
        if (PhoneServer.accelerometerRecent)
            transform.localRotation = new Quaternion(tiltRotation.value.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
    }
}
