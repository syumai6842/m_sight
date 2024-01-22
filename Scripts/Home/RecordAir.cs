using System.Collections;
using System;
using NRKernal;
using NRKernal.NRExamples;
using NRKernal.Record;
using System.IO;
using UnityEngine;
using System.Linq;

using NativeGalleryNamespace;


#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NRKernal.NRExamples.NativeGalleryDataProvider;
#else
    using GalleryDataProvider = NRKernal.NRExamples.MockGalleryDataProvider;
#endif
public class RecordAir : MonoBehaviour
{

    public ResolutionLevel resolutionLevel;
    public LayerMask cullingMask = -1;

    public enum ResolutionLevel
    {
        High,
        Middle,
        Low,
    }
    
    /// <summary> Save the video to Application.persistentDataPath. </summary>
    /// <value> The full pathname of the video save file. </value>
    public string VideoSavePath
    {
        get
        {
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("Nreal_Record_{0}.mp4", timeStamp);
            return Path.Combine("/sdcard/Movies/", filename);
        }
    }

    /// <summary> The video capture. </summary>
    NRVideoCapture m_VideoCapture = null;
    void CreateVideoCapture(Action callback)
    {
        NRVideoCapture.CreateAsync(false, delegate (NRVideoCapture videoCapture)
        {
            NRDebugger.Info("Created VideoCapture Instance!");
            if (videoCapture != null)
            {
                m_VideoCapture = videoCapture;
                callback?.Invoke();
            }
            else
            {
                NRDebugger.Error("Failed to create VideoCapture Instance!");
            }
        });
    }

    public void StartRecord()
    {
        if (m_VideoCapture == null)
        {
            CreateVideoCapture(() =>
            {
                StartVideoCapture();
            });
        }
        else if (m_VideoCapture.IsRecording)
        {
            this.StopVideoCapture();
        }
        else
        {
            this.StartVideoCapture();
        }
    }


    /// <summary> Starts video capture. </summary>
    public void StartVideoCapture()
    {
        if (m_VideoCapture == null || m_VideoCapture.IsRecording)
        {
            NRDebugger.Warning("Can not start video capture!");
            return;
        }

        CameraParameters cameraParameters = new CameraParameters();
        Resolution cameraResolution = GetResolutionByLevel(ResolutionLevel.Middle);
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.frameRate = (int)cameraResolution.refreshRateRatio.value;
        cameraParameters.cameraResolutionWidth = cameraResolution.width;
        cameraParameters.cameraResolutionHeight = cameraResolution.height;
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
        // Set the blend mode.
        cameraParameters.blendMode = BlendMode.VirtualOnly;
        // Set audio state, audio record needs the permission of "android.permission.RECORD_AUDIO",
        // Add it to your "AndroidManifest.xml" file in "Assets/Plugin".
        cameraParameters.audioState = NRVideoCapture.AudioState.ApplicationAudio;

        m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode, true);
    }

    private Resolution GetResolutionByLevel(ResolutionLevel level)
    {
        var resolutions = NRVideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height);
        Resolution resolution = new Resolution();
        switch (level)
        {
            case ResolutionLevel.High:
                resolution = resolutions.ElementAt(0);
                break;
            case ResolutionLevel.Middle:
                resolution = resolutions.ElementAt(1);
                break;
            case ResolutionLevel.Low:
                resolution = resolutions.ElementAt(2);
                break;
            default:
                break;
        }
        return resolution;
    }

    /// <summary> Stops video capture. </summary>
    public void StopVideoCapture()
    {
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            NRDebugger.Warning("Can not stop video capture!");
            return;
        }

        NRDebugger.Info("Stop Video Capture!");
        m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
    }

    /// <summary> Executes the 'started video capture mode' action. </summary>
    /// <param name="result"> The result.</param>
    void OnStartedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Started Video Capture Mode faild!");
            return;
        }

        NRDebugger.Info("Started Video Capture Mode!");
        m_VideoCapture.StartRecordingAsync(VideoSavePath, OnStartedRecordingVideo, NativeConstants.RECORD_VOLUME_MIC, NativeConstants.RECORD_VOLUME_APP);
    }

    /// <summary> Executes the 'started recording video' action. </summary>
    /// <param name="result"> The result.</param>
    void OnStartedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Started Recording Video Faild!");
            return;
        }

        NRDebugger.Info("Started Recording Video!");
        m_VideoCapture.GetContext().GetBehaviour().SetCameraMask(cullingMask.value);
    }

    /// <summary> Executes the 'stopped recording video' action. </summary>
    /// <param name="result"> The result.</param>
    void OnStoppedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            NRDebugger.Info("Stopped Recording Video Faild!");
            return;
        }

        NRDebugger.Info("Stopped Recording Video!");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    /// <summary> Executes the 'stopped video capture mode' action. </summary>
    /// <param name="result"> The result.</param>
    void OnStoppedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
    {
        NRDebugger.Info("Stopped Video Capture Mode!");

        var encoder = m_VideoCapture.GetContext().GetEncoder() as VideoEncoder;
        string path = encoder.EncodeConfig.outPutPath;
        string filename = string.Format("Nreal_Shot_Video_{0}.mp4", NRTools.GetTimeStamp().ToString());

        StartCoroutine(DelayInsertVideoToGallery(path, filename, "Record"));

        // Release video capture resource.
        m_VideoCapture.Dispose();
        m_VideoCapture = null;
    }

    void OnDestroy()
    {
        // Release video capture resource.
        m_VideoCapture?.Dispose();
        m_VideoCapture = null;
    }

    IEnumerator DelayInsertVideoToGallery(string originFilePath, string displayName, string folderName)
    {
        yield return new WaitForSeconds(0.1f);
        InsertVideoToGallery(originFilePath, displayName, folderName);
    }

    GalleryDataProvider galleryDataTool;
    public void InsertVideoToGallery(string originFilePath, string displayName, string folderName)
    {
        NRDebugger.Info("InsertVideoToGallery: {0}, {1} => {2}", displayName, originFilePath, folderName);
        if (galleryDataTool == null)
        {
            galleryDataTool = new GalleryDataProvider();
        }

        galleryDataTool.InsertVideo(originFilePath, displayName, folderName);
    }
}
