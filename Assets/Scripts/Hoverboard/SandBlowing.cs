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
        Vector3 vel = board.GetComponent<Rigidbody>().velocity;
        vel.y = 0;
        float displaceDist = Mathf.Clamp(vel.magnitude * 0.5f, 0, 15);
        Debug.Log(displaceDist);
        if (Physics.Raycast(board.position + vel.normalized * displaceDist, Vector3.down, out hit, 50, LayerMask.GetMask("GravRoad")))
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
