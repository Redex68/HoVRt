using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandBlowing : MonoBehaviour
{
    public Transform board;
    public ParticleSystem dustClouds;
    public ParticleSystem sandParticles;

    private void Update()
    {
        // make sure particle systems emit from the ground
        RaycastHit hit;
        float displaceDist = Mathf.Clamp(board.GetComponent<Rigidbody>().velocity.magnitude * 0.5f, 0, 15);
        Debug.Log(displaceDist);
        if (Physics.Raycast(board.position + board.forward * displaceDist, Vector3.down, out hit, 50, LayerMask.GetMask("GravRoad")))
        {
            dustClouds.transform.position = hit.point;
            sandParticles.transform.position = hit.point;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger touching terrain");
        dustClouds.enableEmission = true;
        sandParticles.enableEmission = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger above terrain");
        dustClouds.enableEmission = false;
        sandParticles.enableEmission = false;
    }
}
