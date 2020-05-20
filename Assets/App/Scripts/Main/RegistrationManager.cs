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

    string m_PersonFacesFolder = "";
    string m_PersonId = "";
    string m_PersonGroup = "";

    int m_NumberOfRequiredFaces = 3;
    int m_PhotosTaken = 0;

    void Start()
    {
        m_StatusManager = FindObjectOfType<StatusManager>();
        m_AzureFaceDetection = FindObjectOfType<AzureFaceDetection>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        m_DetectionManager = GetComponent<DetectionManager>();

        m_PersonFacesFolder = Application.dataPath + Constants.PREFIX_TRAIN_IMAGES_PATH + Constants.PREFIX_TRAIN_IMAGE_NAME;
        ClearTrainFolder();
        m_AzureFaceDetection.OnFacesAddedToPerson += Train;

    }

    /// <summary>
    /// This method is the entry point of control flow from DetectionManager
    /// The capture button functionality is modified to capture faces for this person instead
    /// </summary>
    /// <param name="personGroup"></param>
    /// <param name="personId"></param>
    public void Register(string personGroup, string personId)
    {
        m_StatusManager.ShowStatus("Press button to register photo");
        m_CaptureManager.OnCapture += CaptureFaces;
        m_PersonId = personId;
        m_PersonGroup = personGroup;
        m_PhotosTaken = 0;
    }

    /// <summary>
    /// The method that is called when the capture button is pressed in Registration mode
    /// </summary>
    void CaptureFaces(Texture2D snapshot)
    {
        if (m_PhotosTaken == m_NumberOfRequiredFaces) return;
        string personFolder = Application.dataPath + Constants.PREFIX_TRAIN_IMAGES_PATH + Constants.PREFIX_TRAIN_IMAGE_NAME + m_PersonId;
        if (!Directory.Exists(personFolder))
        {
            Folders.Create(personFolder);
        }
        File.WriteAllBytes(personFolder + "/" + m_PersonId + "_" + m_PhotosTaken.ToString() + ".jpg", snapshot.EncodeToJPG());
        Debug.Log("Capture button pressed during registration");
        m_PhotosTaken++;
        m_StatusManager.ShowStatus("Capturing : " + m_PhotosTaken + " of 3");

        if(m_PhotosTaken == m_NumberOfRequiredFaces)
        {
            RegisterFacesToAzure();
        }
    }

    void RegisterFacesToAzure()
    {
        string personFolder = Application.dataPath + Constants.PREFIX_TRAIN_IMAGES_PATH + Constants.PREFIX_TRAIN_IMAGE_NAME + m_PersonId;
        string[] imageFiles = Directory.GetFiles(personFolder, "*.jpg");
        int numOfRegisteredPhoto = 0;

        if (imageFiles.Length == 0)
        {
            Debug.LogError("No images to be added to Person");
            return;
        }
        StartCoroutine(m_AzureFaceDetection.AddFaceToPersonInGroup(m_PersonGroup, m_PersonId, imageFiles));
    }

    void Train()
    {

    }

    /// <summary>
    /// This method is the bridge to return control flow back to the Detection Manager
    /// </summary>
    void DetectAgain()
    {
        
    }

    void ClearTrainFolder()
    {
        if (Directory.Exists(m_PersonFacesFolder))
        {
            Directory.Delete(m_PersonFacesFolder);
        }
    }
}
