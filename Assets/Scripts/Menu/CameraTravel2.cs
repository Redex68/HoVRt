using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTravel2 : MonoBehaviour
{
    [SerializeField] List<Transform> locations;
    [SerializeField] float panSpeed;
    [SerializeField] CinemachineVirtualCamera cam;

    private bool traveling = false;
    private float speed = 50f;
    private Transform destination = null;
    private CinemachineTrackedDolly dolly;

    void Start()
    {
        dolly = cam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    void Update()
    {
        float distance = float.MaxValue;
        int closest = 0;
        for(int i = 0; i < locations.Count; i++)
        {
            float newDist = (locations[i].position - transform.position).magnitude;
            if(newDist < distance)
            {
                closest = i;
                distance = newDist;
            }
        }
        dolly.m_PathPosition += speed * Time.deltaTime;
        cam.LookAt = locations[closest];
    }
}
