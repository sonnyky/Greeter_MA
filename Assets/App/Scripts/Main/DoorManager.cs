using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    AzureManager m_AzureManager;
    AzureFaceDetection m_AzureFaceDetection;
    CaptureManager m_CaptureManager;

    DetectionManager m_DetectionManager;
    RegistrationManager m_RegistrationManager;

    int m_CurrentStep = Constants.AZURE_CHECK_PERSONGROUP_EXISTS;

    // Start is called before the first frame update
    void Start()
    {
        // Check that all managers are present
        m_AzureManager = FindObjectOfType<AzureManager>();
        m_AzureFaceDetection = FindObjectOfType<AzureFaceDetection>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        if(m_AzureFaceDetection == null || m_AzureManager == null || m_CaptureManager == null)
        {
            Debug.Log(Constants.MANAGERS_NOT_PRESENT);
        }
        else
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        m_DetectionManager = GetComponent<DetectionManager>();
        m_RegistrationManager = GetComponent<RegistrationManager>();
    }

    public void SetCurrentStep(int current)
    {
        m_CurrentStep = current;
    }
}
