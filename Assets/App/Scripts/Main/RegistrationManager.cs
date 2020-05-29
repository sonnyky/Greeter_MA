using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RegistrationManager : MonoBehaviour
{
    AzureFaceDetection m_AzureFaceDetection;
    DetectionManager m_DetectionManager;
    CaptureManager m_CaptureManager;
    StatusManager m_StatusManager;

    string m_PersonId = "";
    string m_PersonGroup = "";

    int m_NumberOfRequiredFaces = 3;
    int m_PhotosTaken = 0;
    List<Texture2D> m_PhotosToRegister;

    WaitForSeconds m_TimeDelayUntilCanCallAzureAgain = new WaitForSeconds(30f);

    void Start()
    {
        m_StatusManager = FindObjectOfType<StatusManager>();
        m_AzureFaceDetection = FindObjectOfType<AzureFaceDetection>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        m_DetectionManager = GetComponent<DetectionManager>();

        m_PhotosToRegister = new List<Texture2D>();
        m_AzureFaceDetection.OnFacesAddedToPerson += Train;

        m_AzureFaceDetection.OnTrainingSuccess = null;
        m_AzureFaceDetection.OnTrainingSuccess += DetectAgain;
    }

    /// <summary>
    /// This method is the entry point of control flow from DetectionManager
    /// The capture button functionality is modified to capture faces for this person instead
    /// </summary>
    /// <param name="personGroup"></param>
    /// <param name="personId"></param>
    public void Register(string personGroup, string personId)
    {
        m_PhotosToRegister.Clear();
        m_CaptureManager.ReenableButton();
        m_StatusManager.ShowStatus("Press button to register photo");
        m_CaptureManager.OnCapture += CaptureFaces;

        m_AzureFaceDetection.OnTrainingSuccess = null;
        m_AzureFaceDetection.OnTrainingSuccess += DetectAgain;

        m_PersonId = personId;
        m_PersonGroup = personGroup;
        m_PhotosTaken = 0;
    }

    /// <summary>
    /// The method that is called when the capture button is pressed in Registration mode
    /// </summary>
    void CaptureFaces(Texture2D snapshot)
    {
        Debug.Log("CaptureFaces");
        m_StatusManager.ShowStatus("CaptureFaces");

        if (m_PhotosTaken == m_NumberOfRequiredFaces)
        {
            Debug.Log("CaptureFaces, PhotosTaken : " + m_PhotosTaken + " and requiredFaces : " + m_NumberOfRequiredFaces);
            return;
        }
        m_PhotosToRegister.Add(snapshot);
        Debug.Log("Capture button pressed during registration");
        m_PhotosTaken++;
        m_StatusManager.ShowStatus("Registering : " + m_PhotosTaken + " of " + m_NumberOfRequiredFaces);

        if(m_PhotosTaken == m_NumberOfRequiredFaces)
        {
            RegisterFacesToAzure();
        }
        else
        {
            m_CaptureManager.ReenableButton();
        }
    }

    void RegisterFacesToAzure()
    {
        StartCoroutine(m_AzureFaceDetection.AddFaceToPersonInGroup(m_PersonGroup, m_PersonId, m_PhotosToRegister));
    }

    void Train()
    {
        StartCoroutine(m_AzureFaceDetection.Train(m_PersonGroup));
    }

    /// <summary>
    /// This method is the bridge to return control flow back to the Detection Manager
    /// </summary>
    void DetectAgain()
    {
        m_StatusManager.ShowStatus("Thank you. Revalidating in 30 secs");
        m_CaptureManager.OnCapture = null;

        m_AzureFaceDetection.OnTrainingSuccess = null;
        m_DetectionManager.Init();
        StartCoroutine(C_DetectAgain());
    }

    IEnumerator C_DetectAgain()
    {
        yield return m_TimeDelayUntilCanCallAzureAgain;
        StartCoroutine(m_AzureFaceDetection.Get(m_PersonGroup));
    }
}
