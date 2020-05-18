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

    public System.Action OnCapture;
    public System.Action OnGroupSelection;

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
        m_StatusManager.ShowStatus(Constants.CAPTURING);
    }

    private void OnApplicationQuit()
    {
        m_WebcamTexture.Stop();
    }
}
