using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;


public class PoseTransformer : MonoBehaviour
{
    [SerializeField] ThreadSafeLandmarksVariable rx; // this sucks
    private Animator animator;
    private Dictionary<LandmarkMap, ModelJoint> mappedLandmarks = new Dictionary<LandmarkMap, ModelJoint>();
    LandmarkMap rootJoint = LandmarkMap.NOSE;

    public ModelJoint[] modelJoints;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < modelJoints.Length; i++) {
            modelJoints[i].bone = animator.GetBoneTransform(((LandmarkMap)i).MapHumanBodyBone());
            modelJoints[i].jointType = (LandmarkMap)i;
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            mappedLandmarks.Add(modelJoints[i].jointType, modelJoints[i]);
        }
        
    }
  
    // Update is called once per frame
    void Update()
    {
        if(rx.value.TryDequeue(out var tmp)) {
            //Debug.Log(tmp);
            UpdateLandmarkMap(tmp);
        }
        ProcessSkeleton();
    }

    void ProcessSkeleton() {
        Vector3 rootPos = Quaternion.Euler(0f, 180f, 0f) * mappedLandmarks[rootJoint].bone.position;
        //transform.position = rootPos;

    }

    public void UpdateLandmarkMap(LandmarkList landmarks) {
        if (landmarks == null) return;
        // this relies heavily on the mediapipe landmarks actually arriving in order 😬
        for(int i = 0; i < landmarks.Landmark.Count; ++i) { // genius naming scheme, LandmarkList.Landmark is ~a List<Landmark>
            Landmark lm = landmarks.Landmark[i];
            mappedLandmarks[(LandmarkMap)i].bone.position = LandmarkToVec3(lm); // surely there's a better way to do this, not every frame
        }
    }

    Vector3 LandmarkToVec3(Landmark lm) {
        return new Vector3(lm.X, lm.Y, lm.Z);
    }

}