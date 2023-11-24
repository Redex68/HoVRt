using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;


public class PoseTransformer : MonoBehaviour
{
    [SerializeField] ThreadSafeLandmarksVariable rx; // this sucks
    private Animator animator;
    private Dictionary<LandmarkMap, Vector3> mappedLandmarks = new Dictionary<LandmarkMap, Vector3>();

    public ModelJoint[] modelJoints;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < modelJoints.Length; i++) {
            modelJoints[i].bone = animator.GetBoneTransform(((LandmarkMap)i).MapHumanBodyBone());
            modelJoints[i].jointType = (LandmarkMap)i;
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            mappedLandmarks.Add(modelJoints[i].jointType, modelJoints[i].bone.position);
        }
        
    }
  
    // Update is called once per frame
    void Update()
    {
        if(rx.value.TryDequeue(out var tmp)) {
            //Debug.Log(tmp);
            UpdateLandmarkMap(tmp);
        }
        MoveRig(animator.avatarRoot);
        transform.position = (mappedLandmarks[LandmarkMap.LEFT_FOOT_INDEX] + mappedLandmarks[LandmarkMap.RIGHT_FOOT_INDEX]) / 2f;
    }

    void MoveRig(Transform bone) {
        for (int i = 0; i < bone.childCount; ++i) {
            Transform child = bone.GetChild(i);
            child.position = -FindPos(child.position, child); // why do we need to negate here?
            // child.rotation = FindRot(child.rotation, bone_identifier);
            MoveRig(child);
        }
    }    
    Vector3 FindPos(Vector3 default_position, Transform bone) {
        if (bone == animator.GetBoneTransform(HumanBodyBones.Neck)) {
            return (mappedLandmarks[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks[LandmarkMap.RIGHT_SHOULDER]) / 2f;
        } 
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Head)) {
            return mappedLandmarks[LandmarkMap.NOSE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Hips)) {
            return (mappedLandmarks[LandmarkMap.LEFT_HIP] + mappedLandmarks[LandmarkMap.RIGHT_HIP]) / 2f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Spine)) {
            return (mappedLandmarks[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks[LandmarkMap.RIGHT_SHOULDER]) * 0.25f + (mappedLandmarks[LandmarkMap.LEFT_HIP] + mappedLandmarks[LandmarkMap.RIGHT_HIP]) * 0.75f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)) {
            return mappedLandmarks[LandmarkMap.LEFT_ELBOW];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightLowerArm)) {
            return mappedLandmarks[LandmarkMap.RIGHT_ELBOW];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftHand)) {
            return mappedLandmarks[LandmarkMap.LEFT_WRIST];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightHand)) {
            return mappedLandmarks[LandmarkMap.RIGHT_WRIST];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Chest)) {
            return (mappedLandmarks[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks[LandmarkMap.RIGHT_SHOULDER] + mappedLandmarks[LandmarkMap.LEFT_HIP] + mappedLandmarks[LandmarkMap.RIGHT_HIP]) / 4f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftShoulder)) {
            return mappedLandmarks[LandmarkMap.LEFT_SHOULDER];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightShoulder)) {
            return mappedLandmarks[LandmarkMap.RIGHT_SHOULDER];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)) {
            return mappedLandmarks[LandmarkMap.LEFT_SHOULDER]; // should maybe have an offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightUpperArm)) {
            return mappedLandmarks[LandmarkMap.RIGHT_SHOULDER]; // probably all of these should have an offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) {
            return mappedLandmarks[LandmarkMap.LEFT_HIP]; // may need offset 
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightUpperLeg)) {
            return mappedLandmarks[LandmarkMap.RIGHT_HIP]; // may need offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) {
            return mappedLandmarks[LandmarkMap.LEFT_KNEE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightLowerLeg)) {
            return mappedLandmarks[LandmarkMap.RIGHT_KNEE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftFoot)) {
            return mappedLandmarks[LandmarkMap.LEFT_ANKLE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightFoot)) {
            return mappedLandmarks[LandmarkMap.RIGHT_ANKLE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftToes)) {
            return mappedLandmarks[LandmarkMap.LEFT_FOOT_INDEX];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightToes)) {
            return mappedLandmarks[LandmarkMap.RIGHT_FOOT_INDEX];
        }
        else {
            return -default_position; // everything else seems to be negated, so this is needed for some reason
        }
        
    }

    public void UpdateLandmarkMap(LandmarkList landmarks) {
        if (landmarks == null) return;
        // this relies heavily on the mediapipe landmarks actually arriving in order 😬
        for(int i = 0; i < landmarks.Landmark.Count; ++i) { // genius naming scheme, LandmarkList.Landmark is ~a List<Landmark>
            Landmark lm = landmarks.Landmark[i];
            mappedLandmarks[(LandmarkMap)i] = LandmarkToVec3(lm); 
        }
    }

    Vector3 LandmarkToVec3(Landmark lm) {
        return new Vector3(lm.X, lm.Y, lm.Z);
    }

}