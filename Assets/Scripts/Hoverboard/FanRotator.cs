using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FanRotator : MonoBehaviour
{
    [SerializeField] float speed;

    void Update()
    {
        Quaternion rot = transform.localRotation;
        rot.z += speed;
        transform.localRotation = rot;
    }
}
