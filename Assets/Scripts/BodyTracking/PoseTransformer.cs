using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;


public class PoseTransformer : MonoBehaviour
{
    [SerializeField] SkeletonTrackingSolution sts; // this sucks
    private Animator animator;
    private Dictionary<LandmarkMap, Transform> mappedLandmarks = new Dictionary<LandmarkMap, Transform>();

    Transform hip, neck;


    private Dictionary<HumanBodyBones, CalibrationData> parentCalibrationData = new Dictionary<HumanBodyBones, CalibrationData>();
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Quaternion targetRot;
    private CalibrationData spineUpDown, hipsTwist,chest,head;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        for (int i = 0; i < 33; ++i) {
            mappedLandmarks[(LandmarkMap)i] = new GameObject().transform;
        }
        hip = new GameObject().transform;
        neck = new GameObject().transform;
        sts._doSomethingWithLandmarkList += UpdateLandmarkMap;
    }
    public void Calibrate()
    {
        // Here we store the values of variables required to do the correct rotations at runtime.

        parentCalibrationData.Clear();

        // Manually setting calibration data for the spine chain as we want really specific control over that.
        hip.position = (mappedLandmarks[LandmarkMap.LEFT_HIP].position + mappedLandmarks[LandmarkMap.RIGHT_HIP].position) / 2f;
        neck.position = (mappedLandmarks[LandmarkMap.LEFT_SHOULDER].position + mappedLandmarks[LandmarkMap.RIGHT_SHOULDER].position) / 2f;
        spineUpDown = new CalibrationData(animator.GetBoneTransform(HumanBodyBones.Spine), animator.GetBoneTransform(HumanBodyBones.Neck),
            hip, neck);
        hipsTwist = new CalibrationData(animator.GetBoneTransform(HumanBodyBones.Hips), animator.GetBoneTransform(HumanBodyBones.Hips),
            mappedLandmarks[LandmarkMap.RIGHT_HIP], mappedLandmarks[LandmarkMap.LEFT_HIP]);
        chest = new CalibrationData(animator.GetBoneTransform(HumanBodyBones.Chest), animator.GetBoneTransform(HumanBodyBones.Chest),
            mappedLandmarks[LandmarkMap.RIGHT_HIP], mappedLandmarks[LandmarkMap.LEFT_HIP]);
        head = new CalibrationData(animator.GetBoneTransform(HumanBodyBones.Neck), animator.GetBoneTransform(HumanBodyBones.Head),
            neck, mappedLandmarks[LandmarkMap.NOSE]);

        // Adding calibration data automatically for the rest of the bones.
        AddCalibration(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
            mappedLandmarks[LandmarkMap.RIGHT_SHOULDER], mappedLandmarks[LandmarkMap.RIGHT_ELBOW]);
        AddCalibration(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
            mappedLandmarks[LandmarkMap.RIGHT_ELBOW], mappedLandmarks[LandmarkMap.RIGHT_WRIST]);

        AddCalibration(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
            mappedLandmarks[LandmarkMap.RIGHT_HIP], mappedLandmarks[LandmarkMap.RIGHT_KNEE]);
        AddCalibration(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
            mappedLandmarks[LandmarkMap.RIGHT_KNEE], mappedLandmarks[LandmarkMap.RIGHT_ANKLE]);

        AddCalibration(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
            mappedLandmarks[LandmarkMap.LEFT_SHOULDER], mappedLandmarks[LandmarkMap.LEFT_ELBOW]);
        AddCalibration(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            mappedLandmarks[LandmarkMap.LEFT_ELBOW], mappedLandmarks[LandmarkMap.LEFT_WRIST]);

        AddCalibration(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
            mappedLandmarks[LandmarkMap.LEFT_HIP], mappedLandmarks[LandmarkMap.LEFT_KNEE]);
        AddCalibration(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
            mappedLandmarks[LandmarkMap.LEFT_KNEE], mappedLandmarks[LandmarkMap.LEFT_ANKLE]);

        //if (footTracking)
        {
            AddCalibration(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
                mappedLandmarks[LandmarkMap.LEFT_ANKLE], mappedLandmarks[LandmarkMap.LEFT_FOOT_INDEX]);
            AddCalibration(HumanBodyBones.RightFoot, HumanBodyBones.RightToes,
                mappedLandmarks[LandmarkMap.RIGHT_ANKLE], mappedLandmarks[LandmarkMap.RIGHT_FOOT_INDEX]);
        }

        animator.enabled = false; // disable animator to stop interference.
    }
    private void AddCalibration(HumanBodyBones parent, HumanBodyBones child, Transform trackParent,Transform trackChild)
    {
        parentCalibrationData.Add(parent, 
            new CalibrationData(animator.GetBoneTransform(parent), animator.GetBoneTransform(child),
            trackParent,trackChild));
    }

    // Update is called once per frame
    void Update()
    {
        
        UpdateRig();
    }

    public void UpdateLandmarkMap(LandmarkList landmarks) {
        if (landmarks == null) {
            Debug.Log("no landmark");
            return;
        }
        // this relies heavily on the mediapipe landmarks actually arriving in order 😬
        for(int i = 0; i < landmarks.Landmark.Count; ++i) { // genius naming scheme, LandmarkList.Landmark is ~a List<Landmark>
            Landmark lm = landmarks.Landmark[i];
            mappedLandmarks[(LandmarkMap)i].position = LandmarkToVec3(lm); // surely there's a better way to do this, not every frame
        }
    }

    Vector3 LandmarkToVec3(Landmark lm) {
        return new Vector3(lm.X, lm.Y, lm.Z);
    }

    void UpdateRig() {
        // Adjust the vertical position of the avatar to keep it approximately grounded.
        if(parentCalibrationData.Count > 0)
        {
            float displacement = 0;
            //TEMP
            float footGroundOffset = 0.1f;
            LayerMask ground = 1 << LayerMask.NameToLayer("ground");
            //PMET
            RaycastHit h1;
            if (Physics.Raycast(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, Vector3.down, out h1, 100f, ground, QueryTriggerInteraction.Ignore)){
                displacement = (h1.point - animator.GetBoneTransform(HumanBodyBones.LeftFoot).position).y;
            }
            if (Physics.Raycast(animator.GetBoneTransform(HumanBodyBones.RightFoot).position, Vector3.down, out h1, 100f, ground, QueryTriggerInteraction.Ignore)){
                float displacement2 = (h1.point - animator.GetBoneTransform(HumanBodyBones.RightFoot).position).y;
                if (Mathf.Abs(displacement2) < Mathf.Abs(displacement))
                {
                    displacement = displacement2;
                }
            }
            transform.position = Vector3.Lerp(transform.position,initialPosition+ Vector3.up * displacement + Vector3.up * footGroundOffset,
                Time.deltaTime*5f);
        }

        // Compute the new rotations for each limbs of the avatar using the calibration datas we created before.
        foreach(var i in parentCalibrationData)
        {
            Quaternion deltaRotTracked = Quaternion.FromToRotation(i.Value.initialDir, i.Value.CurrentDirection);
            i.Value.parent.rotation = deltaRotTracked * i.Value.initialRotation;
        }

        // Deal with spine chain as a special case.
        if(parentCalibrationData.Count > 0)
        {
            Vector3 hd = head.CurrentDirection;
            // Some are partial rotations which we can stack together to specify how much we should rotate.
            Quaternion headr = Quaternion.FromToRotation(head.initialDir, hd);
            Quaternion twist = Quaternion.FromToRotation(hipsTwist.initialDir, 
                Vector3.Slerp(hipsTwist.initialDir,hipsTwist.CurrentDirection,.25f));
            Quaternion updown = Quaternion.FromToRotation(spineUpDown.initialDir,
                Vector3.Slerp(spineUpDown.initialDir, spineUpDown.CurrentDirection, .25f));

            // Compute the final rotations.
            Quaternion h = updown * updown * updown * twist * twist;
            Quaternion s = h * twist * updown;
            Quaternion c = s * twist * twist;
            float speed = 10f;
            hipsTwist.Tick(h * hipsTwist.initialRotation, speed);
            spineUpDown.Tick(s * spineUpDown.initialRotation, speed);
            chest.Tick(c * chest.initialRotation, speed);
            head.Tick(updown * twist * headr * head.initialRotation, speed);

            // For additional responsiveness, we rotate the entire transform slightly based on the hips.
            Vector3 d = Vector3.Slerp(hipsTwist.initialDir, hipsTwist.CurrentDirection, .25f);
            d.y *= 0.5f;
            Quaternion deltaRotTracked = Quaternion.FromToRotation(hipsTwist.initialDir, d);
            targetRot= deltaRotTracked * initialRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * speed);
        }
    }

}
/// <sumemary>
/// Cache various values which will be reused during the runtime.
/// </summary>
class CalibrationData
{
    public Transform parent, child,tparent,tchild;
    public Vector3 initialDir;
    public Quaternion initialRotation;

    public Quaternion targetRotation;
    public void Tick(Quaternion newTarget, float speed)
    {
        parent.rotation = newTarget;
        parent.rotation = Quaternion.Lerp(parent.rotation, targetRotation, Time.deltaTime * speed);
    }

    public Vector3 CurrentDirection => (tchild.position - tparent.position).normalized;

    public CalibrationData(Transform fparent, Transform fchild,Transform tparent,Transform tchild)
    {
        initialDir = (tchild.position - tparent.position).normalized;
        initialRotation = fparent.rotation;
        this.parent = fparent;
        this.child = fchild;
        this.tparent = tparent;
        this.tchild = tchild;
    }
}
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
    NONE = 40
}