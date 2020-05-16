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

    [SerializeField]
    Text m_PathText;

    int m_CascadeInitialized = 1;
    bool m_IsCapturing = false;

    [SerializeField]
    RawImage m_DetectionScreenImage;
   
    CvPlugin m_CvPlugin;

    string m_CascadeFileString = "";

    // Start is called before the first frame update
    void Start()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        m_DetectionScreenImage.texture = webcamTexture;
        m_DetectionScreenImage.material.mainTexture = webcamTexture;
        webcamTexture.Play();

#if UNITY_ANDROID && !UNITY_EDITOR
        m_CvPlugin = GetComponent<CvPlugin>();
        m_StatusText.text = "Ver: " + m_CvPlugin.GetOpenCvVersion();
        StartCoroutine("GetXmlFile");
#endif
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
                m_StatusText.text = "Cascade : " + m_CascadeInitialized;

                if(m_CascadeInitialized == 0)
                {
                    m_IsCapturing = true;
                }

            }
        }
    }

    private void OnApplicationQuit()
    {
        m_IsCapturing = false;
    }
}
