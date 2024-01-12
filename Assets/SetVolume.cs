using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolume : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter emitter;
    // Start is called before the first frame update

    public float volume = 0.4f;
    void Awake()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.EventInstance.setVolume(volume);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
