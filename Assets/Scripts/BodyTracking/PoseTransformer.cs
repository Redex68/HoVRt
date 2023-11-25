using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;


public class PoseTransformer : MonoBehaviour
{
    [SerializeField] LandmarksVariable mappedLandmarks;
    private Animator animator;
    public ModelJoint[] modelJoints;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < modelJoints.Length; i++) {
            modelJoints[i].bone = animator.GetBoneTransform(((LandmarkMap)i).MapHumanBodyBone());
            modelJoints[i].jointType = (LandmarkMap)i;
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            mappedLandmarks.value.Add(modelJoints[i].jointType, modelJoints[i].bone.position);
        }
        
    }
  

    void Update()
    {
        MoveRig(animator.avatarRoot);
        transform.position = (mappedLandmarks.value[LandmarkMap.LEFT_FOOT_INDEX] + mappedLandmarks.value[LandmarkMap.RIGHT_FOOT_INDEX]) / 2f;
    }

    void MoveRig(Transform bone) {
        bone.position = -FindPos(bone.position, bone);
        //bone.rotation = FindRot(bone.rotation, bone);
        for (int i = 0; i < bone.childCount; ++i) {
            Transform child = bone.GetChild(i);
            MoveRig(child);
        }
    }    
    Vector3 FindPos(Vector3 default_position, Transform bone) {
        if (bone == animator.GetBoneTransform(HumanBodyBones.Neck)) {
            return (mappedLandmarks.value[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks.value[LandmarkMap.RIGHT_SHOULDER]) / 2f;
        } 
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Head)) {
            return mappedLandmarks.value[LandmarkMap.NOSE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Hips)) {
            return (mappedLandmarks.value[LandmarkMap.LEFT_HIP] + mappedLandmarks.value[LandmarkMap.RIGHT_HIP]) / 2f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Spine)) {
            return (mappedLandmarks.value[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks.value[LandmarkMap.RIGHT_SHOULDER]) * 0.25f + (mappedLandmarks.value[LandmarkMap.LEFT_HIP] + mappedLandmarks.value[LandmarkMap.RIGHT_HIP]) * 0.75f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_ELBOW];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightLowerArm)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_ELBOW];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftHand)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_WRIST];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightHand)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_WRIST];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.Chest)) {
            return (mappedLandmarks.value[LandmarkMap.LEFT_SHOULDER] + mappedLandmarks.value[LandmarkMap.RIGHT_SHOULDER] + mappedLandmarks.value[LandmarkMap.LEFT_HIP] + mappedLandmarks.value[LandmarkMap.RIGHT_HIP]) / 4f;
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftShoulder)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_SHOULDER];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightShoulder)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_SHOULDER];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_SHOULDER]; // should maybe have an offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightUpperArm)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_SHOULDER]; // probably all of these should have an offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_HIP]; // may need offset 
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightUpperLeg)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_HIP]; // may need offset
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_KNEE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightLowerLeg)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_KNEE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftFoot)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_ANKLE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightFoot)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_ANKLE];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.LeftToes)) {
            return mappedLandmarks.value[LandmarkMap.LEFT_FOOT_INDEX];
        }
        else if (bone == animator.GetBoneTransform(HumanBodyBones.RightToes)) {
            return mappedLandmarks.value[LandmarkMap.RIGHT_FOOT_INDEX];
        }
        else {
            return -default_position; // everything else seems to be negated, so this is needed for some reason
        }
        
    }

}