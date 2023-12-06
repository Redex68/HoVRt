using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    [SerializeField] GameEvent checkpointAdded;
    [SerializeField] GameEvent checkpointPassed;
    [SerializeField] List<GameObject> particleSystems;

    private bool passed = false;

    void Start()
    {
        checkpointAdded.Raise(this, null);
    }

    void OnTriggerEnter()
    {
        if(!passed)
        {
            passed = true;
            checkpointPassed.SimpleRaise();
            Passed();
        }
    }

    private void Passed()
    {
        foreach(GameObject particleSystem in particleSystems)
        {
            particleSystem.transform.parent = null;
            particleSystem.transform.localScale = Vector3.one;
            particleSystem.GetComponent<ParticleSystem>().Play();
            Destroy(particleSystem, 10f);
        }
        Destroy(gameObject);
    }
}
