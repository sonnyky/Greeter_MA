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

    WebcamManager m_WebcamManager;
    WebCamTexture m_WebcamTexture;

    public System.Action<Texture2D> OnCapture;

    // Start is called before the first frame update
    void Start()
    {
        m_WebcamManager = GetComponent<WebcamManager>();
        InitializeCamera();
        m_CaptureButton.onClick.AddListener(() =>
        {
            Capture();
        });
    }

    void InitializeCamera()
    {
        string cameraName = m_WebcamManager.GetFrontCameraName();
        if (!cameraName.Equals("default"))
        {
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

    public void Capture()
    {
        Texture2D snapshot = new Texture2D(m_WebcamTexture.width, m_WebcamTexture.height);
        snapshot.SetPixels32(m_WebcamTexture.GetPixels32());
        snapshot.Apply();

        m_StatusManager.ShowStatus(Constants.CAPTURING);
        if(OnCapture != null)
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

}
