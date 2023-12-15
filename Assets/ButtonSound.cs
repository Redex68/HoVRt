using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ButtonSound : MonoBehaviour
{
    [SerializeField]
    public FMODUnity.EventReference soundEvent; // Drag and drop the FMOD sound event into this field in the Inspector

    [SerializeField]
    public float action = 0.0f; // Match this with the parameter name in FMOD Studio

    public string actionParameterName = "Menu Action"; // Match this with the parameter name in FMOD Studio

    public void Play()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Play the sound and set the pitch parameter
            FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(soundEvent);
            instance.setParameterByName(actionParameterName, action);
            instance.start();
            instance.release(); // Release the instance when done (it will keep playing)
        }
    }
}

