using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{
    CaptureManager m_CaptureManager;
    AzureFaceDetection m_AzureFaceDetection;
    RegistrationManager m_RegistrationManager;

    string m_RuntimeImage;

    string m_PersonGroupId = "default";
    float m_Timeout = 10f; // Wait 10 seconds before declaring network problems

    // Start is called before the first frame update
    void Start()
    {
        m_AzureFaceDetection = FindObjectOfType<AzureFaceDetection>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        m_RegistrationManager = GetComponent<RegistrationManager>();
        // Subscribe to dropdown UI to get person group ID. Not critical for the demo
        // TODO

        // When capture button is pressed, start detection process
        m_CaptureManager.OnCapture += Entry;

        // Subscribe to the Azure face detection component
        m_AzureFaceDetection.OnPersonGroupNotExisted += CreatePersonGroup;
        m_AzureFaceDetection.OnPersonGroupExists += GetPersonListInGroup;
        m_AzureFaceDetection.OnPersonGroupCreated += GetPersonListInGroup;
        m_AzureFaceDetection.OnPersonListNotEmpty += CheckAllPersonsHaveFaces;
        m_AzureFaceDetection.OnPersonListEmpty += CreatePersonInGroup;
        m_AzureFaceDetection.OnPersonInGroupDeleted += GetPersonListInGroup;
        m_AzureFaceDetection.OnPersonCreated += StartRegistration;
    }

    void Entry(Texture2D snapshot)
    {
        if(!Directory.Exists(Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH))
        {
            Folders.Create(Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH);
        }
        m_RuntimeImage = Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH;
        System.IO.File.WriteAllBytes(m_RuntimeImage + "main.jpg", snapshot.EncodeToJPG());
        StartCoroutine(m_AzureFaceDetection.Get(m_PersonGroupId));
    }

    void CreatePersonGroup()
    {
        StartCoroutine(m_AzureFaceDetection.Create(m_PersonGroupId));
    }

    void GetPersonListInGroup()
    {
        StartCoroutine(m_AzureFaceDetection.GetPersonList(m_PersonGroupId));
    }

    void CheckAllPersonsHaveFaces(List<PersonInGroup.Person> list)
    {
        if(list.Count == 0)
        {
            CreatePersonInGroup();
        }

        int personChecked = 0;
        for(int i=0; i<list.Count; i++)
        {
            if(list[i].persistedFaceIds.Length == 0)
            {
                StartCoroutine(m_AzureFaceDetection.DeletePersonInGroup(m_PersonGroupId, list[i].personId));
                break;
            }
            else
            {
                personChecked++;
            }
        }

        // We managed to loop through the Person list and not find a Person without Faces
        if(personChecked == list.Count)
        {
            GetPersonGroupTrainingStatus();
        }
    }
    
    /// <summary>
    /// This method creates a person and if successful
    /// it passes flow to the Registration Manager.
    /// </summary>
    void CreatePersonInGroup()
    {
        StartCoroutine(m_AzureFaceDetection.CreatePersonInGroup(m_PersonGroupId, System.DateTime.Now.ToString(), "Auto person"));
    }

    /// <summary>
    /// Passes the control flow to the Registration manager
    /// </summary>
    void StartRegistration(string personId)
    {
        m_CaptureManager.OnCapture = null;
        m_RegistrationManager.Register(m_PersonGroupId, personId);
    }

    void GetPersonGroupTrainingStatus()
    {

    }

}
