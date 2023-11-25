using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/ThreadSafeLandmarksVariable")]
public class ThreadSafeLandmarksVariable : ScriptableObject
{
    public ConcurrentQueue<LandmarkList> value = new();
}
