using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skateboard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {            
        Vector3 translated = new Vector3(-PhoneServer.accelerometerData.y, PhoneServer.accelerometerData.z, PhoneServer.accelerometerData.x);

        if(PhoneServer.accelerometerRecent)
            transform.rotation = Tilt.tiltRotation;
    }
}
