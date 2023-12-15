using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderDestination : MonoBehaviour
{
    [SerializeField] private Transform spiderTransform;
    private NavMeshAgent spiderAgent;

    private void Start()
    {
        spiderAgent = spiderTransform.GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SetSpiderDestination();
        }
    }

    private Vector3 ChoosePosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void SetSpiderDestination()
    {
        Vector3 targetPosition = ChoosePosition();
        if (targetPosition != Vector3.zero)
        {
            spiderAgent.destination = targetPosition;
        }
    }
}
