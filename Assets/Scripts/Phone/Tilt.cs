using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilt : MonoBehaviour
{

    public static Quaternion tiltRotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PhoneServer.accelerometerRecent)
        {
            Vector3 translated = new Vector3(-PhoneServer.accelerometerData.y, PhoneServer.accelerometerData.z, PhoneServer.accelerometerData.x);
            tiltRotation = Quaternion.FromToRotation(Vector3.down, translated.normalized);
            Debug.Log(tiltRotation);
            Debug.Log(PhoneServer.accelerometerData);
        }
    }
}
