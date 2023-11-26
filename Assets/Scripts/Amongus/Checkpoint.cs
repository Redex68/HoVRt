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
        passed = true;
        checkpointPassed.SimpleRaise();
    }
}
