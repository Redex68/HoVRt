using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelJoint
   {
       /// <summary> Transform model bone </summary>
       public Transform bone = null;
       public LandmarkMap jointType = LandmarkMap.NONE;

       //For "Direct translation"
       public LandmarkMap parentJointType = LandmarkMap.NONE;
       /// <summary> Base model bones rotation offsets</summary>

       public Quaternion baseRotOffset
       {
           get;
           set;
       }

       public Transform parentBone
       {
           get;
           set;
       }

       // <summary> Base distance to parent bone </summary>
       public float baseDistanceToParent
       {
           get;
           set;
       }
   }