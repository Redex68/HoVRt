using UnityEngine;

public abstract class ScriptableVariable<T>: ScriptableObject {
    [SerializeField] T defaultValue;
    [Tooltip("Whether the default value will be set each time the game is reloaded")]
    [SerializeField] bool useDefault;
    public T value;

    void OnEnable()
    {
        if(useDefault) value = defaultValue;
    }
}