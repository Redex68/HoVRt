using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandBlowing : MonoBehaviour
{
    public ParticleSystem particles;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger touching terrain");
        particles.enableEmission = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger above terrain");
        particles.enableEmission = false;
    }
}
