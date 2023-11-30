using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe;
using Unity.Mathematics;

public class LandmarkUpdater : MonoBehaviour
{
    [SerializeField] ThreadSafeLandmarksVariable rx;
    [SerializeField] LandmarksVariable mappedLandmarks;
    [SerializeField] FloatVariable crouchPercentage;

    float maxTop = 0, minBot = 0;
    // Start is called before the first frame update
    void Start()
    {
       
    }
    Vector3 LandmarkToVec3(Landmark lm) {
        return new Vector3(lm.X, lm.Y, lm.Z);
    }
    public void UpdateLandmarkMap(LandmarkList landmarks) {
        if (landmarks == null) return;
        // this relies heavily on the mediapipe landmarks actually arriving in order 😬
        // (They do, it's fine)
        for(int i = 0; i < landmarks.Landmark.Count; ++i) { // genius naming scheme, LandmarkList.Landmark is ~a List<Landmark>
            Landmark lm = landmarks.Landmark[i];
            mappedLandmarks.value[(LandmarkMap)i] = LandmarkToVec3(lm); 
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(rx.value.TryDequeue(out var tmp)) {
            //Debug.Log(tmp);
            UpdateLandmarkMap(tmp);
        }
        
        float top = -mappedLandmarks.value.GetValueOrDefault(LandmarkMap.NOSE, Vector3.zero).y;
        float bot = Mathf.Min(-mappedLandmarks.value.GetValueOrDefault(LandmarkMap.LEFT_ANKLE, Vector3.zero).y, -mappedLandmarks.value.GetValueOrDefault(LandmarkMap.RIGHT_ANKLE, Vector3.zero).y);
        if (top > maxTop) {
            maxTop = top;
        }
        if (bot < minBot) {
            minBot = bot;
        }
        float maxHeight = maxTop - minBot;
        // Debug.Log($"{top - bot}, {(top - bot)/maxHeight}");
        crouchPercentage.value = Mathf.Abs(top - bot) / maxHeight; // this will be NaN until the camera has initialized fully
    }
}
