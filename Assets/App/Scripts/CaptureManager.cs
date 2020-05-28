using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CaptureManager : MonoBehaviour
{
    enum CaptureState {
        STANDBY,
        CAPTURE
    }

    [SerializeField]
    StatusManager m_StatusManager;

    [SerializeField]
    Button m_CaptureButton;

    [SerializeField]
    RawImage m_DetectionScreenImage;
    Quaternion baseRotation;

    WebcamManager m_WebcamManager;
    WebCamTexture m_WebcamTexture;
    CvPlugin m_NativeCvPlugin;

    public System.Action<Texture2D> OnCapture;

    WaitForSeconds m_WaitTimeAfterCameraStart = new WaitForSeconds(3f);

    Color32[] inputPixelData;
    Color32[] processedPixelData;
    GCHandle inputPixelHandle;
    IntPtr inputPixelPtr;
    GCHandle processedPixelHandle;
    IntPtr processedPixelPtr;

    bool m_ButtonActive = true;

    // Start is called before the first frame update
    void Start()
    {
        m_WebcamManager = GetComponent<WebcamManager>();
        m_NativeCvPlugin = GetComponent<CvPlugin>();
        InitializeCamera();
        m_CaptureButton.onClick.AddListener(() =>
        {
            CapturePrep();
        });
    }

    void InitializeCamera()
    {
        string cameraName = m_WebcamManager.GetFrontCameraName();
        if (!cameraName.Equals("default"))
        {
            baseRotation = transform.rotation;
            m_WebcamTexture = new WebCamTexture(cameraName);
            m_DetectionScreenImage.texture = m_WebcamTexture;
            m_DetectionScreenImage.material.mainTexture = m_WebcamTexture;
            m_WebcamTexture.Play();
        }
        else
        {
            Debug.LogError("No front facing cameras found");
        }
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_DetectionScreenImage.transform.rotation = baseRotation * Quaternion.AngleAxis(m_WebcamTexture.videoRotationAngle, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up);
#endif
    }

    public void CapturePrep()
    {
        if (!m_ButtonActive) return;
        m_StatusManager.ShowStatus(Constants.CAPTURING);

        if (!m_WebcamTexture.isPlaying)
        {
            m_WebcamTexture.Play();
        }

        m_ButtonActive = false;
        m_CaptureButton.gameObject.SetActive(false);
        StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        yield return m_WaitTimeAfterCameraStart;
        Texture2D snapshot = new Texture2D(m_WebcamTexture.width, m_WebcamTexture.height);

        // Android devices gives images rotated by 90 degrees. So we rotate it again to detect faces properly.
#if UNITY_ANDROID && !UNITY_EDITOR
        Color32[] data = m_WebcamTexture.GetPixels32();
        m_NativeCvPlugin.Flip(ref data, m_WebcamTexture.width, m_WebcamTexture.height, 90);
        snapshot.SetPixels32(data);
#endif
#if UNITY_EDITOR
        snapshot.SetPixels32(m_WebcamTexture.GetPixels32());
#endif
        snapshot.Apply();

        if (OnCapture != null)
        {
            OnCapture.Invoke(snapshot);
        }
    }

    public void StopCamera()
    {
        m_WebcamTexture.Stop();
    }

    private void OnApplicationQuit()
    {
        m_WebcamTexture.Stop();
    }

    // Public interfaces

    public void StopCapture()
    {
        m_WebcamTexture.Stop();
    }

    public void StartCapture()
    {
        m_WebcamTexture.Play();
    }

    public bool IsCameraActive()
    {
        return m_WebcamTexture.isPlaying;
    }

    public void ReenableButton()
    {
        m_ButtonActive = true;
        m_CaptureButton.gameObject.SetActive(true);
    }

}
