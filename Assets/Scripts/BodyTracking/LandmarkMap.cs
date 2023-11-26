
using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum LandmarkMap
{
    NOSE = 0,
    LEFT_EYE_INNER = 4, // 1
    LEFT_EYE = 5, // 2
    LEFT_EYE_OUTER = 6, // 3
    RIGHT_EYE_INNER = 1, // 4
    RIGHT_EYE = 2, // 5
    RIGHT_EYE_OUTER = 3, // 6
    LEFT_EAR = 8, // 7
    RIGHT_EAR = 7, // 8
    MOUTH_LEFT = 10, // 9
    MOUTH_RIGHT = 9, // 10
    LEFT_SHOULDER = 12, // 11
    RIGHT_SHOULDER = 11, // 12
    LEFT_ELBOW = 14, // 13
    RIGHT_ELBOW = 13, // 14
    LEFT_WRIST = 16, // 15
    RIGHT_WRIST = 15, // 16
    LEFT_PINKY = 18, // 17
    RIGHT_PINKY = 17, // 18
    LEFT_INDEX = 20, // 19
    RIGHT_INDEX = 19, // 20
    LEFT_THUMB = 22, // 21
    RIGHT_THUMB = 21, // 22
    LEFT_HIP = 24, // 23
    RIGHT_HIP = 23, // 24
    LEFT_KNEE = 26, // 25
    RIGHT_KNEE = 25, // 26
    LEFT_ANKLE = 28, // 27
    RIGHT_ANKLE = 27, // 28
    LEFT_HEEL = 30, // 29
    RIGHT_HEEL = 29, // 30
    LEFT_FOOT_INDEX = 32, // 31
    RIGHT_FOOT_INDEX = 31, // 32
    NONE = 40,

}

static class LandmarkMapMethods {
    public static HumanBodyBones MapHumanBodyBone(this LandmarkMap m) {
        switch (m) {
            case LandmarkMap.NOSE:
            case LandmarkMap.LEFT_EYE_INNER:
            case LandmarkMap.LEFT_EYE:
            case LandmarkMap.LEFT_EYE_OUTER:
            case LandmarkMap.RIGHT_EYE_INNER:
            case LandmarkMap.RIGHT_EYE:
            case LandmarkMap.RIGHT_EYE_OUTER:
            case LandmarkMap.LEFT_EAR:
            case LandmarkMap.RIGHT_EAR:
            case LandmarkMap.MOUTH_LEFT:
            case LandmarkMap.MOUTH_RIGHT :
                return HumanBodyBones.Head;
            case LandmarkMap.LEFT_SHOULDER:
                return HumanBodyBones.LeftShoulder;
            case LandmarkMap.RIGHT_SHOULDER :
                return HumanBodyBones.RightShoulder;
            case LandmarkMap.LEFT_ELBOW:
                return HumanBodyBones.LeftLowerArm;
            case LandmarkMap.RIGHT_ELBOW:
                return HumanBodyBones.RightLowerArm;
            case LandmarkMap.LEFT_WRIST:
                return HumanBodyBones.LeftHand;
            case LandmarkMap.RIGHT_WRIST:
                return HumanBodyBones.RightHand;
            case LandmarkMap.LEFT_PINKY:
                return HumanBodyBones.LeftLittleProximal;
            case LandmarkMap.RIGHT_PINKY:
                return HumanBodyBones.RightLittleProximal;
            case LandmarkMap.LEFT_INDEX:
                return HumanBodyBones.LeftIndexProximal;
            case LandmarkMap.RIGHT_INDEX:
                return HumanBodyBones.RightIndexProximal;
            case LandmarkMap.LEFT_THUMB:
                return HumanBodyBones.LeftThumbProximal;
            case LandmarkMap.RIGHT_THUMB:
                return HumanBodyBones.RightThumbProximal;
            case LandmarkMap.LEFT_HIP :
                return HumanBodyBones.Hips;
            case LandmarkMap.RIGHT_HIP:
                return HumanBodyBones.Hips;
            case LandmarkMap.LEFT_KNEE:
                return HumanBodyBones.LeftLowerLeg;
            case LandmarkMap.RIGHT_KNEE:
                return HumanBodyBones.RightLowerLeg;
            case LandmarkMap.LEFT_ANKLE:
                return HumanBodyBones.LeftFoot;
            case LandmarkMap.RIGHT_ANKLE :
                return HumanBodyBones.RightFoot;
            case LandmarkMap.LEFT_HEEL:
                return HumanBodyBones.LeftFoot;
            case LandmarkMap.RIGHT_HEEL :
                return HumanBodyBones.RightFoot;
            case LandmarkMap.LEFT_FOOT_INDEX :
                return HumanBodyBones.LeftFoot;
            case LandmarkMap.RIGHT_FOOT_INDEX:
                return HumanBodyBones.RightFoot;
            case LandmarkMap.NONE:
                return HumanBodyBones.LastBone;
        }
        return HumanBodyBones.LastBone;
    }
}