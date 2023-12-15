using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DestinationPicker : MonoBehaviour
{
    [SerializeField] List<Transform> destinations;
    [SerializeField] NavMeshAgent spiderAgent;

    void Update()
    {
        if(spiderAgent.isOnNavMesh && spiderAgent.remainingDistance < 0.5f)
            spiderAgent.SetDestination(destinations[Random.Range(0, destinations.Count)].position);
    }
}
