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


    void Awake()
    {
        var emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.EventInstance.setVolume(volume);
    }

    void Update()
    {
        float effectiveRPM = Mathf.Lerp(minRPM, maxRPM, speed);
        var emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.SetParameter("RPM", effectiveRPM);
        emitter.EventInstance.setVolume(volume);
    }
}