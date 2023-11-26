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

[CreateAssetMenu(menuName = "Scriptable Object/Int Variable")]
public class IntVariable : ScriptableVariable<int>
{
}

[CreateAssetMenu(menuName = "Scriptable Object/Bool Variable")]
public class BoolVariable : ScriptableVariable<bool>
{
}

[CreateAssetMenu(menuName = "Scriptable Object/Vector3 Variable")]
public class Vector3Variable : ScriptableVariable<Vector3>
{
}

[CreateAssetMenu(menuName = "Scriptable Object/Quaternion Variable")]
public class QuaternionVariable : ScriptableVariable<Quaternion>
{
}

[CreateAssetMenu(menuName = "Scriptable Object/Float Variable")]
public class FloatVariable : ScriptableVariable<float>
{
}
