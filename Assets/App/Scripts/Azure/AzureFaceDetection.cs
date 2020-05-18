using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AzureManager))]
public class AzureFaceDetection : MonoBehaviour
{
    bool personGroupExists = false;
    bool createPersonGroupSucceded = false;
    bool personListRetrieved = false;
    bool personGroupTrained = false;
    Training.TrainingStatus m_TrainingStatus;
    Error.ErrorResponse m_Error;
    AzureManager m_AzureManager;

    string m_Endpoint = "";
    string m_ApiKey = "";
    string m_PersonGroupId = "default";

    List<PersonInGroup.Person> m_ListOfPersonsInGroup;

    public System.Action<int> OnError;
    public System.Action OnPersonGroupCreated;

    private void Start()
    {
        m_Error = new Error.ErrorResponse();
        m_TrainingStatus = new Training.TrainingStatus();
        m_ListOfPersonsInGroup = new List<PersonInGroup.Person>();
        m_AzureManager = GetComponent<AzureManager>();
        m_ApiKey = m_AzureManager.GetApiKey();
        m_Endpoint = m_AzureManager.GetEndpoint();
    }

    public void SetPersonGroupId(string personGroupId)
    {
        m_PersonGroupId = personGroupId;
    }

    public void Detect()
    {
        StartCoroutine(Get(m_PersonGroupId));
    }

    void Create()
    {
        StartCoroutine(Create(m_PersonGroupId));
    }

    IEnumerator Get(string personGroup)
    {
        yield return RequestManager.GetPersonGroup(m_Endpoint, m_ApiKey, personGroup, value => personGroupExists = value);
        if (personGroupExists)
        {
            StartCoroutine(GetPersonList(personGroup));
        }
        else
        {
            StartCoroutine(Create(personGroup));
        }
    }

    IEnumerator Create(string personGroup)
    {
        yield return RequestManager.CreatePersonGroup(m_Endpoint, m_ApiKey, personGroup, personGroup, "Auto created",
           value => createPersonGroupSucceded = value);

        if (createPersonGroupSucceded)
        {
            // Notify the caller to start face registration
            if (OnPersonGroupCreated != null)
                OnPersonGroupCreated.Invoke();
        }
        else
        {
            Debug.LogError(Constants.AZURE_PERSON_GROUP_CREATION_FAILED);
            if (OnError != null)
                OnError.Invoke(Constants.AZURE_PERSON_GROUP_CREATION_FAILED_CODE);
        }

    }

    IEnumerator GetPersonList(string personGroup)
    {
        yield return RequestManager.GetPersonListInGroup(m_Endpoint, m_ApiKey, personGroup, value => personListRetrieved = value, res => m_ListOfPersonsInGroup = res);
        if (personListRetrieved)
        {
            StartCoroutine(GetTrainingStatus(personGroup));
        }
        else
        {
            Debug.LogError(Constants.AZURE_PERSONGROUPLIST_EMPTY_OR_ERROR);
            if (OnError != null)
                OnError.Invoke(Constants.AZURE_PERSONGROUPLIST_EMPTY_OR_ERROR_CODE);
        }
    }

    IEnumerator GetTrainingStatus(string personGroup)
    {
        yield return RequestManager.GetPersonGroupTrainingStatus(m_Endpoint, m_ApiKey, personGroup, result => personGroupTrained = result, err => m_Error = err,  value => m_TrainingStatus = value);
        if (!personGroupTrained && m_Error.error.code.Equals("PersonGroupNotTrained"))
        {
            //TODO : Train!
        }
        else if(personGroupTrained && m_TrainingStatus.status.Equals("succeeded"))
        {
            //TODO : Detect!
        }
        else
        {
            // TODO : Throw error!
        }
    }
}
