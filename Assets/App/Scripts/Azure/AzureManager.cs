using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class AzureManager : Singleton
{
    BaseManager m_Base;

    StatusManager m_StatusManager;
    CaptureManager m_CaptureManager;

    string m_ApiKey = Constants.NONE;
    string m_Endpoint = "https://japaneast.api.cognitive.microsoft.com/face/v1.0";
    bool m_Ready = false;

    AzureFaceDetection m_FaceDetection;

    // Start is called before the first frame update
    void Start()
    {
        m_Base = FindObjectOfType<BaseManager>();
        switch (m_Base.GetState())
        {
            case BaseManager.AppState.INITIALIZE:
                Initializations();
                break;
            case BaseManager.AppState.STANDBY:
                break;
            case BaseManager.AppState.REGISTRATION:
                break;
        }
    }

    void Initializations()
    {
        m_StatusManager = FindObjectOfType<StatusManager>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        if (!m_StatusManager || !m_CaptureManager)
        {
            Debug.LogError(Constants.UI_MANAGER_NOT_FOUND);
        }
        else
        {
          
#if UNITY_ANDROID && !UNITY_EDITOR
            StartCoroutine("GetSettings");
#endif
#if UNITY_EDITOR
            m_ApiKey = EnvironmentVariables.GetVariable("GREETER_AZURE_FACE_API_KEY");
#endif
            if (m_ApiKey != null && !m_ApiKey.Equals(Constants.NONE))
            {
                m_StatusManager.ShowStatus("Ready");
                m_Ready = true;
            }
        }
    }

    private IEnumerator GetSettings()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "settings.txt");
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat(this, "Unable to load file due to {0} - {1}", www.responseCode, www.error);
                m_StatusManager.ShowStatus(Constants.API_KEY_NOT_LOADED);
            }
            else
            {
                string[] settings = www.downloadHandler.text.Split(',');
                m_Endpoint = settings[0];
                m_ApiKey = settings[1];
                m_StatusManager.ShowStatus(Constants.API_KEY_LOADED);
                m_Ready = true;
            }
        }
    }
    public string GetApiKey()
    {
        return m_ApiKey;
    }

    public string GetEndpoint()
    {
        return m_Endpoint;
    }
    
}
