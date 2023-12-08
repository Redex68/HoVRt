using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTravel : MonoBehaviour
{
    [SerializeField] List<Transform> locations;
    [SerializeField] float panSpeed;

    private bool traveling = false;
    private float speed = 0f;
    private Transform destination = null;

    void Start()
    {
        if(locations.Count != 0) StartCoroutine(PickNextDestination());
    }

    void Update()
    {
        if(traveling)
        {
            Vector3 dir = destination.position - transform.position;
            float toMove = panSpeed * Time.deltaTime;
            if(toMove > dir.magnitude) toMove = dir.magnitude;
            transform.position += dir.normalized * toMove;

            if((transform.position - destination.position).magnitude < 0.1f)
            {
                traveling = false;
                StartCoroutine(PanAround());
            }
        }
    }

    private IEnumerator PanAround()
    {
        float rotated = 0f;
        while(rotated < 180)
        {
            float delta = 60 * Time.deltaTime;
            rotated += delta;
            transform.Rotate(0, delta, 0);
            yield return new WaitForNextFrameUnit();
        }
        
        StartCoroutine(PickNextDestination());
    }

    private IEnumerator PickNextDestination()
    {
        yield return new WaitForSeconds(2f);
        Transform newDest = null;
        do
        {
            newDest = locations[Random.Range(0, locations.Count)];
        } while(newDest == destination);
        destination = newDest;
        traveling = true;
    }
}
