using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverBoardTilter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PhoneServer.accelerometerRecent)
            transform.localRotation = new Quaternion(Tilt.tiltRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
    }
}
