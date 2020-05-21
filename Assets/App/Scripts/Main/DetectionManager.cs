using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{
    StatusManager m_StatusManager;
    CaptureManager m_CaptureManager;
    AzureFaceDetection m_AzureFaceDetection;
    RegistrationManager m_RegistrationManager;

    WaitForSeconds m_TimeUntilCameraStops = new WaitForSeconds(5f);

    string m_RuntimeImage="";

    string m_PersonGroupId = "default";
    float m_Timeout = 10f; // Wait 10 seconds before declaring network problems

    List<PersonInGroup.Person> m_PersonsInGroup;

    // Start is called before the first frame update
    void Start()
    {
        m_PersonsInGroup = new List<PersonInGroup.Person>();
        m_RuntimeImage = Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH + "main.jpg";
        m_StatusManager = FindObjectOfType<StatusManager>();
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
        m_AzureFaceDetection.OnPersonGroupNotTrained += Train;
        m_AzureFaceDetection.OnPersonGroupTrained += DetermineFaceArea;
        m_AzureFaceDetection.OnTrainingSuccess += DetermineFaceArea;
        m_AzureFaceDetection.OnFacesNotFound += RestartFlow;
        m_AzureFaceDetection.OnFacesFound += Identify;
        m_AzureFaceDetection.OnFaceNotIdentified += CreatePersonInGroup;
        m_AzureFaceDetection.OnFaceIdentified += CheckIdentifiedFaceIsKnown;
    }

    public void Init()
    {
        m_CaptureManager.OnCapture += Entry;
        m_PersonsInGroup.Clear();
    }

    void Entry(Texture2D snapshot)
    {
        m_PersonsInGroup.Clear();
        m_StatusManager.ShowStatus("Attempting to recognize face");
        if(!Directory.Exists(Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH))
        {
            Folders.Create(Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH);
        }
       
        System.IO.File.WriteAllBytes(m_RuntimeImage, snapshot.EncodeToJPG());
        StartCoroutine(m_AzureFaceDetection.Get(m_PersonGroupId));
    }

    void CreatePersonGroup()
    {
        StartCoroutine(m_AzureFaceDetection.Create(m_PersonGroupId));
    }

    void GetPersonListInGroup()
    {
        m_StatusManager.ShowStatus("Getting person list");
        StartCoroutine(m_AzureFaceDetection.GetPersonList(m_PersonGroupId));
    }

    void CheckAllPersonsHaveFaces(List<PersonInGroup.Person> list)
    {
        m_PersonsInGroup.Clear();
        m_PersonsInGroup.AddRange(list);
        if (list.Count == 0)
        {
            Debug.Log("not all persons have faces");
            CreatePersonInGroup();
        }

        int personChecked = 0;
        for(int i=0; i<list.Count; i++)
        {
            if(list[i].persistedFaceIds.Length == 0)
            {
                Debug.Log("deleting face : " + list[i].personId);
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
            Debug.Log("get person group training status");
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
        StartCoroutine(m_AzureFaceDetection.GetTrainingStatus(m_PersonGroupId));
    }

    void Train()
    {
        StartCoroutine(m_AzureFaceDetection.Train(m_PersonGroupId));
    }

    void DetermineFaceArea()
    {
        StartCoroutine(m_AzureFaceDetection.DetermineFaceArea(m_PersonGroupId, m_RuntimeImage));
    }

    void RestartFlow()
    {
        m_StatusManager.ShowStatus(Constants.RESTART_DETECTION_FLOW);
    }

    void Identify(List<FacesBasic.FacesDetectionResponse> faces)
    {
        StartCoroutine(m_AzureFaceDetection.Identify(m_PersonGroupId, faces.ToArray()));
    }

    void CheckIdentifiedFaceIsKnown(List<IdentifiedFaces.IdentifiedFacesResponse> identifiedFaces)
    {
        int index = 0;
        float conf = 0f;

        if(identifiedFaces.Count > 1)
        {
            RestartFlow(); // because we want to detect one person at a time
        }
        else
        {
            for(int i=0; i<identifiedFaces[0].candidates.Length; i++)
            {
                if(identifiedFaces[0].candidates[i].confidence > conf)
                {
                    conf = identifiedFaces[0].candidates[i].confidence;
                    index = i;
                }
            }
            string personIdToIdentify = identifiedFaces[0].candidates[index].personId;

            bool personKnown = false;
            for (int j=0; j<m_PersonsInGroup.Count; j++)
            {
                if (personIdToIdentify.Equals(m_PersonsInGroup[j].personId))
                {
                    personKnown = true;
                }
            }

            if (personKnown)
            {
                m_StatusManager.ShowStatus(Constants.PERSON_KNOWN);
                m_StatusManager.SetDetectionIcon(0);
                StartCoroutine(StopCamera());
            }
            else
            {
                m_StatusManager.ShowStatus(Constants.PERSON_UNKNOWN);
                m_StatusManager.SetDetectionIcon(1);
                CreatePersonInGroup();
            }
        }
    }

    IEnumerator StopCamera()
    {
        yield return m_TimeUntilCameraStops;
        m_CaptureManager.StopCamera();
    }
}
