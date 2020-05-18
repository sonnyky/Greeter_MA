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
    Text m_StatusText;

    [SerializeField]
    RawImage m_DetectionScreenImage;

    WebcamManager m_WebcamManager;

    public System.Action OnCapture;

    // Start is called before the first frame update
    void Start()
    {
        m_WebcamManager = GetComponent<WebcamManager>();
        InitializeCamera();
       

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine("GetApiKey");
#endif
    }

    void InitializeCamera()
    {
        string cameraName = m_WebcamManager.GetFrontCameraName();
        if (!cameraName.Equals("default"))
        {
            WebCamTexture webcamTexture = new WebCamTexture(cameraName);
            m_DetectionScreenImage.texture = webcamTexture;
            m_DetectionScreenImage.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }
        else
        {
            Debug.LogError("No front facing cameras found");
        }
    }

    private IEnumerator GetApiKey()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "apikey.txt");
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat(this, "Unable to load file due to {0} - {1}", www.responseCode, www.error);
                m_StatusText.text = "Loading API key failed";
            }
            else
            {
                m_StatusText.text = "API Key loaded";
            }
        }
    }

    private void OnApplicationQuit()
    {
    }
}
