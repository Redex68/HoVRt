using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] GameEvent checkpointAdded;
    [SerializeField] GameEvent checkpointPassed;

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
        }
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        float time = Time.time;
        while(Time.time - time < 10)
        {
            transform.position += transform.up * 100 * Time.deltaTime;
            yield return new WaitForEndOfFrameUnit();
        }
    }
}
