using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderRotator : MonoBehaviour
{
    [SerializeField] LayerMask ground;
    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50f, ground))
        {
            //Doesn't work properly
            transform.rotation = transform.rotation * Quaternion.FromToRotation(transform.up, hit.normal);
        }
    }
}
