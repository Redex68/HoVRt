using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    [SerializeField] GameEvent checkpointAdded;
    [SerializeField] GameEvent checkpointPassed;
    [SerializeField] List<GameObject> particleSystems;

    private FMODUnity.StudioEventEmitter emitter;
    private bool passed = false;

    [Range(0.0f, 10.0f)]
    public float volume = 0.4f;

    void Awake()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.EventInstance.setVolume(volume);
    }
    void Start()
    {
        checkpointAdded.Raise(this, null);
    }

    void OnTriggerEnter()
    {
        if (!passed)
        {
            passed = true;
            checkpointPassed.SimpleRaise();
            Passed();
        }
    }

    private void Passed()
    {
        emitter.SetParameter("Activate", 0.0f);
        emitter.EventInstance.setVolume(volume);
        foreach (GameObject particleSystem in particleSystems)
        {
            particleSystem.transform.parent = null;
            particleSystem.transform.localScale = Vector3.one;
            particleSystem.GetComponent<ParticleSystem>().Play();
            Destroy(particleSystem, 10f);
        }
        Destroy(gameObject);
    }
}





