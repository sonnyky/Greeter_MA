using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CvPlugin))]
public class CaptureManager : MonoBehaviour
{
    [SerializeField]
    Text m_StatusText;

    int m_CascadeInitialized = 1;
    bool m_IsCapturing = false;
    bool m_IsCapturingEditor = false;

    [SerializeField]
    RawImage m_DetectionScreenImage;

    WebcamManager m_WebcamManager;
   
    CvPlugin m_CvPlugin;

    string m_CascadeFileString = "";

    WebCamTexture m_WebcamTexture;
    Color32[] data;
    Texture2D m_OriginalTexture;
    Texture2D m_ProcessedTexture;

    // Start is called before the first frame update
    void Start()
    {
     
      
       m_WebcamManager = GetComponent<WebcamManager>();
       InitializeCamera();
       

#if UNITY_ANDROID && !UNITY_EDITOR
       m_CvPlugin = GetComponent<CvPlugin>();
       m_StatusText.text = "Ver: " + m_CvPlugin.GetOpenCvVersion();
       StartCoroutine("GetXmlFile");
#endif
    }

    void InitializeCamera()
    {
        string cameraName = m_WebcamManager.GetFrontCameraName();
        if (!cameraName.Equals("default"))
        {
            m_WebcamTexture = new WebCamTexture(cameraName);
            data = new Color32[m_WebcamTexture.width * m_WebcamTexture.height];
            m_WebcamTexture.Play();

#if UNITY_ANDROID && !UNITY_EDITOR
            m_ProcessedTexture = new Texture2D(m_WebcamTexture.width, m_WebcamTexture.height);
            m_DetectionScreenImage.material.mainTexture = m_ProcessedTexture;
            m_DetectionScreenImage.texture = m_ProcessedTexture;
#endif
#if UNITY_EDITOR
            m_OriginalTexture = new Texture2D(m_WebcamTexture.width, m_WebcamTexture.height);
            m_DetectionScreenImage.material.mainTexture = m_OriginalTexture;
            m_DetectionScreenImage.texture = m_OriginalTexture;
            m_IsCapturingEditor = true;
#endif
        }
        else
        {
            Debug.LogError("No front facing cameras found");
        }
    }

    private void Update()
    {
        if (m_IsCapturingEditor)
        {
            m_OriginalTexture.SetPixels(m_WebcamTexture.GetPixels());
            m_OriginalTexture.Apply();
        }
        if (m_IsCapturing)
        {
            ProcessImage();
        }
    }   
  
    void ProcessImage()
    {
        Texture2D processed = new Texture2D(m_DetectionScreenImage.texture.width, m_DetectionScreenImage.texture.height);
        m_CvPlugin.Detect((Texture2D)m_DetectionScreenImage.texture, ref processed, m_DetectionScreenImage.texture.width, m_DetectionScreenImage.texture.height);
        m_DetectionScreenImage.texture = processed;
        m_DetectionScreenImage.material.mainTexture = processed;
    }

    private IEnumerator GetXmlFile()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "lbpcascade_frontalface.xml");
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat(this, "Unable to load file due to {0} - {1}", www.responseCode, www.error);
                m_StatusText.text = "Loading XML status failed";
            }
            else
            {
                m_CascadeFileString = www.downloadHandler.text;
                Debug.Log("GreeterLog : " + m_CascadeFileString);
                m_CascadeInitialized = m_CvPlugin.InitializeCascade(m_CascadeFileString);

                if(m_CascadeInitialized == 0)
                {
                    m_IsCapturing = true;
                    m_StatusText.text = "Capture: " + m_IsCapturing;
                }

            }
        }
    }

    private void OnApplicationQuit()
    {
        m_IsCapturing = false;
    }
}
