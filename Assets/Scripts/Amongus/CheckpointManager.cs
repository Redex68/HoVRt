using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text text;

    private int checkpointsPassed = 0;
    private int numCheckpoints = 0;

    public void OnCheckpointAdded()
    {
        numCheckpoints++;
        text.text = $"{checkpointsPassed}/{numCheckpoints}";
    }

    public void OnCheckpointPassed()
    {
        checkpointsPassed++;
        text.text = $"{checkpointsPassed}/{numCheckpoints}";
    }
}
