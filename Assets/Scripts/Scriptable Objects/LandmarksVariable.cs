using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/LandmarksVariable")]
public class LandmarksVariable : ScriptableObject
{
    public Dictionary<LandmarkMap, Vector3> value = new();
}

