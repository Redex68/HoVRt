using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.PoseTracking;
using UnityEngine;

public class SkeletonTrackingSolution : ImageSourceSolution<PoseTrackingGraph>
{


    // TODO: actually figure out what we want here. It needs to be thread safe ;_;
    public event Action<LandmarkList> _doSomethingWithLandmarkList;
    
    public ThreadSafeLandmarksVariable tx;

    [SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
    [SerializeField] private RectTransform _worldAnnotationArea;

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
        graphRunner.AddTextureFrameToInputStream(textureFrame);
    }

    protected override void SetupScreen(ImageSource imageSource)
    {
        base.SetupScreen(imageSource);
        _worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
    }

    protected override void OnStartRun()
    {
        if (!runningMode.IsSynchronous()) {
            graphRunner.OnPoseWorldLandmarksOutput += OnWorldLandmarksOutput;
        }
        if (_poseWorldLandmarksAnnotationController != null){
            ImageSource imageSource = ImageSourceProvider.ImageSource;
            SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
        }
    }

    protected override IEnumerator WaitForNextValue()
    {
        LandmarkList poseWorldLandmarks = null;

        Detection _poseDetection = null;
        NormalizedLandmarkList _poseLandmarks = null;
        ImageFrame _segmentationMask = null;
        NormalizedRect _roiFromLandmarks = null;

        if (runningMode == RunningMode.Sync)
        {
          var _ = graphRunner.TryGetNext(out _poseDetection, out _poseLandmarks, out poseWorldLandmarks, out _segmentationMask, out _roiFromLandmarks, true);
        }
        else if (runningMode == RunningMode.NonBlockingSync)
        {
          yield return new WaitUntil(() => graphRunner.TryGetNext(out _poseDetection, out _poseLandmarks, out poseWorldLandmarks, out _segmentationMask, out _roiFromLandmarks, false));
        }

        _doSomethingWithLandmarkList?.Invoke(poseWorldLandmarks);
    }


    void OnWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
        _poseWorldLandmarksAnnotationController?.DrawLater(eventArgs.value);
        _doSomethingWithLandmarkList?.Invoke(eventArgs.value);
        //Debug.Log($"Got value {eventArgs.value}");
        tx.value.Enqueue(eventArgs.value);
    }

}
