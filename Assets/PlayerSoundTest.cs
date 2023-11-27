using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundTest : MonoBehaviour
{

    public float minRPM = 0;
    public float maxRPM = 9000;

    [Range(0.0f, 1.0f)]
    public float speed = 0;

    [Range(0.0f, 10.0f)]
    public float volume = 0.1f;
    
    private FMODUnity.StudioEventEmitter emitter;

    void Awake()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.EventInstance.setVolume(volume);
    }

    void Update()
    {
        float effectiveRPM = Mathf.Lerp(minRPM, maxRPM, speed);
        emitter.SetParameter("RPM", effectiveRPM);
        emitter.EventInstance.setVolume(volume);
    }

    void OnDisable()
    {
        emitter.EventInstance.setVolume(0);
    }
}