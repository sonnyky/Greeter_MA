﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DetectionManager : MonoBehaviour
{
    StatusManager m_StatusManager;
    CaptureManager m_CaptureManager;
    AzureFaceDetection m_AzureFaceDetection;
    RegistrationManager m_RegistrationManager;
    SoundManager m_SoundManager;

    WaitForSeconds m_TimeUntilCameraStops = new WaitForSeconds(5f);
    WaitForSeconds m_TimeUntilCanCallAzureAgain = new WaitForSeconds(30f);

    string m_RuntimeImage="";
    Texture2D runtimeShot;
    bool m_NewRegistered = false;

    string m_PersonGroupId = "default";
    float m_Timeout = 10f; // Wait 10 seconds before declaring network problems

    List<PersonInGroup.Person> m_PersonsInGroup;

    // Start is called before the first frame update
    void Start()
    {
        runtimeShot = new Texture2D(1, 1);

        m_PersonsInGroup = new List<PersonInGroup.Person>();
        m_RuntimeImage = Application.dataPath + Constants.PREFIX_DETECTION_IMAGES_PATH + "main.jpg";
        m_StatusManager = FindObjectOfType<StatusManager>();
        m_SoundManager = FindObjectOfType<SoundManager>();
        m_AzureFaceDetection = FindObjectOfType<AzureFaceDetection>();
        m_CaptureManager = FindObjectOfType<CaptureManager>();
        m_RegistrationManager = GetComponent<RegistrationManager>();
        // Subscribe to dropdown UI to get person group ID. Not critical for the demo
        // TODO

        // When capture button is pressed, start detection process
        m_CaptureManager.OnCapture += Entry;

        // When debug button is pressed, delete all Person Groups
        m_CaptureManager.OnDebug += DebugReset;

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

        // Debug
        m_AzureFaceDetection.OnPersonGroupDeleted += DebugSuccessful;
    }

    public void Init()
    {
        Debug.Log("capture manager on capture set to Entry");
        m_CaptureManager.OnCapture += Entry;
        m_PersonsInGroup.Clear();
        m_AzureFaceDetection.OnTrainingSuccess += DetermineFaceArea;
    }

    void Entry(Texture2D snapshot)
    {
        m_PersonsInGroup.Clear();
        m_NewRegistered = false;
        m_StatusManager.ShowStatus("Attempting to recognize face");
        runtimeShot = snapshot;
        StartCoroutine(m_AzureFaceDetection.Get(m_PersonGroupId));
    }

    void CreatePersonGroup()
    {
        StartCoroutine(m_AzureFaceDetection.Create(m_PersonGroupId));
    }

    void GetPersonListInGroup()
    {
        m_StatusManager.ShowStatus("Getting person list");
        Debug.Log("Getting person list");
        StartCoroutine(m_AzureFaceDetection.GetPersonList(m_PersonGroupId));
    }

    void CheckAllPersonsHaveFaces(List<PersonInGroup.Person> list)
    {
        Debug.Log("CheckAllPersonsHaveFaces : " + list.Count);
        m_PersonsInGroup.Clear();
        if (list.Count == 0)
        {
            Debug.Log("not all persons have faces");
            CreatePersonInGroup();
            return;
        }
        m_PersonsInGroup.AddRange(list);

        Debug.Log("Got list of persons in group : " + list.Count);
        Debug.Log("Member variable of persons in group : " + m_PersonsInGroup.Count);

        int personChecked = 0;
        for(int i=0; i<list.Count; i++)
        {
            if(list[i].persistedFaceIds.Length == 0)
            {
                Debug.Log("deleting face : " + list[i].personId);
                StartCoroutine(m_AzureFaceDetection.DeletePersonInGroup(m_PersonGroupId, list[i].personId));
                return;
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
        m_NewRegistered = true;
        m_SoundManager.PlayClip(Constants.PERSON_UNKNOWN_CODE);
        m_StatusManager.SetDetectionIcon(1);
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
        StartCoroutine(m_AzureFaceDetection.DetermineFaceArea(m_PersonGroupId, runtimeShot));
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
            Debug.Log("candidates length : " + identifiedFaces[0].candidates.Length);

            if(identifiedFaces[0].candidates.Length == 0)
            {
                Debug.Log("First identified face doesn't contain a candidate");
                m_StatusManager.ShowStatus(Constants.PERSON_UNKNOWN);
                m_SoundManager.PlayClip(Constants.PERSON_UNKNOWN_CODE);
                m_StatusManager.SetDetectionIcon(1);
                CreatePersonInGroup();
                return;
            }

            string personIdToIdentify = identifiedFaces[0].candidates[index].personId;

            bool personKnown = false;
            Debug.Log("How many persons in group ? " + m_PersonsInGroup.Count);
            for (int j=0; j<m_PersonsInGroup.Count; j++)
            {
                if (personIdToIdentify.Equals(m_PersonsInGroup[j].personId))
                {
                    personKnown = true;
                }
            }

            if (personKnown)
            {
                switch (m_NewRegistered)
                {
                    case true:
                        m_StatusManager.ShowStatus(Constants.NEW_PERSON_REGISTERED);
                        StartCoroutine(ReportFaceVerificationResult("New Person Registered"));
                        break;
                    case false:
                        m_StatusManager.ShowStatus(Constants.PERSON_KNOWN);
                        StartCoroutine(ReportFaceVerificationResult("Registered Person"));
                        break;
                }
                m_SoundManager.PlayClip(Constants.PERSON_KNOWN_CODE);
                m_StatusManager.SetDetectionIcon(0);
                StartCoroutine(StopCamera());
            }
            else
            {
                m_StatusManager.ShowStatus(Constants.PERSON_UNKNOWN);
                CreatePersonInGroup();
            }
        }
    }

    IEnumerator StopCamera()
    {
        yield return m_TimeUntilCameraStops;
        m_CaptureManager.StopCamera();

        yield return m_TimeUntilCanCallAzureAgain;
        m_CaptureManager.ReenableButton();
    }

    IEnumerator ReportFaceVerificationResult(string result)
    {
        string request = "https://fathomless-oasis-34994.herokuapp.com/api/azure_face/identification/identified/" + result;
        var www = new UnityWebRequest(request, "POST");
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            m_StatusManager.ShowStatus("Network error. Wait 30 secs and try again");
        }
        else
        {
            Debug.Log("response : " + www.downloadHandler.text);
            m_StatusManager.ShowStatus("Notification sent. Wait 30 secs before next verification");
        }
    }

    void DebugReset()
    {
        StartCoroutine(m_AzureFaceDetection.DeletePersonGroup(m_PersonGroupId));
    }

    void DebugSuccessful()
    {
        Debug.Log("Debug Successful");
        m_StatusManager.ShowDebugStatus("Persons Reset");
        StartCoroutine("RestoreDebugText");
    }

    IEnumerator RestoreDebugText()
    {
        yield return new WaitForSeconds(3f);
        m_StatusManager.ShowDebugStatus("Press to delete all data");
    }
}
