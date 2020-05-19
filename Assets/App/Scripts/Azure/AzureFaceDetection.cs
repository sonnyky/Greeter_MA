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
    bool personCreated = false;
    bool trainPersonGroupResult = false;
    string trainPersonGroupError = "";
    Training.TrainingStatus m_TrainingStatus;
    PersonCreateSuccess.PersonCreateSuccessResponse m_PersonCreatedSuccess;
    Error.ErrorResponse m_Error;
    AzureManager m_AzureManager;

    string m_Endpoint = "";
    string m_ApiKey = "";
    string m_PersonGroupId = "default";

    List<PersonInGroup.Person> m_ListOfPersonsInGroup;
    int numOfPersonsInGroup;

    public System.Action<int> OnUnknownError;

    public System.Action OnPersonGroupExists;
    public System.Action OnPersonGroupNotExisted;
    public System.Action OnPersonGroupCreated;
    public System.Action OnPersonListEmpty;
    public System.Action<List<PersonInGroup.Person>> OnPersonListNotEmpty;
    public System.Action OnPersonListInGroupCreated;
    public System.Action OnPersonListHaveFaces;
    public System.Action OnFacesAddedToPerson;
    public System.Action OnPersonGroupTrained;
    public System.Action OnPersonGroupNotTrained;
    public System.Action OnPersonInGroupDeleted;
    public System.Action<string> OnPersonCreated;
    public System.Action OnTrainingSuccess;

    private void Start()
    {
        m_Error = new Error.ErrorResponse();
        m_PersonCreatedSuccess = new PersonCreateSuccess.PersonCreateSuccessResponse();
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

    public IEnumerator Get(string personGroup)
    {
        yield return RequestManager.GetPersonGroup(m_Endpoint, m_ApiKey, personGroup, value => personGroupExists = value);
        if (personGroupExists && OnPersonGroupExists != null)
        {
            OnPersonGroupExists.Invoke();
        }
        else
        {
            if (OnPersonGroupNotExisted != null) OnPersonGroupNotExisted.Invoke();
        }
    }

    public IEnumerator Create(string personGroup)
    {
        yield return RequestManager.CreatePersonGroup(m_Endpoint, m_ApiKey, personGroup, personGroup, "Auto created",
           value => createPersonGroupSucceded = value);

        if (createPersonGroupSucceded)
        {
            if (OnPersonGroupCreated != null)
                OnPersonGroupCreated.Invoke();
        }
    }

    public IEnumerator GetPersonList(string personGroup)
    {
        m_ListOfPersonsInGroup.Clear();
        yield return RequestManager.GetPersonListInGroup(m_Endpoint, m_ApiKey, personGroup, value => personListRetrieved = value, res => m_ListOfPersonsInGroup = res);
        if (personListRetrieved && m_ListOfPersonsInGroup.Count > 0)
        {
            numOfPersonsInGroup = m_ListOfPersonsInGroup.Count;
            if (OnPersonListNotEmpty != null) OnPersonListNotEmpty.Invoke(m_ListOfPersonsInGroup);
        }
        else
        {
            if (OnPersonListEmpty != null) OnPersonListEmpty.Invoke();
        }
    }

    public IEnumerator CreatePersonInGroup(string personGroup, string name, string userData)
    {
        yield return RequestManager.CreatePersonInGroup(m_Endpoint, m_ApiKey, personGroup, name, userData, value => personCreated = value, success => m_PersonCreatedSuccess = success);
        if (personCreated)
        {
            if (OnPersonCreated != null) OnPersonCreated.Invoke(m_PersonCreatedSuccess.personId);
        }
    }

    public IEnumerator GetTrainingStatus(string personGroup)
    {
        yield return RequestManager.GetPersonGroupTrainingStatus(m_Endpoint, m_ApiKey, personGroup, result => personGroupTrained = result, err => m_Error = err,  value => m_TrainingStatus = value);
        if (!personGroupTrained && m_Error.error.code.Equals(Constants.AZURE_PERSONGROUPNOTTRAINED_CODE) && OnPersonGroupNotTrained != null)
        {
            OnPersonGroupNotTrained.Invoke();
        }
        else if(personGroupTrained && m_TrainingStatus.status.Equals(Constants.AZURE_PERSONGROUPTRAINSUCCESS) && OnPersonGroupTrained != null)
        {
           OnPersonGroupTrained.Invoke();
        }
    }

    /// <summary>
    /// Trains a person group. Since we want to make sure that there are faces added to every person in the person group,
    /// this method should only be run after checking the validity of the persons in the group.
    /// </summary>
    /// <param name="personGroup"></param>
    /// <returns></returns>
    public IEnumerator Train(string personGroup)
    {
        yield return RequestManager.TrainPersonGroup(m_Endpoint, m_ApiKey, personGroup, value => trainPersonGroupResult = value, text => trainPersonGroupError = text);
        if (trainPersonGroupResult && OnTrainingSuccess != null)
        {
            OnTrainingSuccess.Invoke();
        }
    }


    public IEnumerator DeletePersonInGroup(string personGroup, string personId)
    {
        bool successfulDelete = false;
        string deleteError = "";
        yield return RequestManager.DeletePersonInGroup(m_Endpoint, m_ApiKey, personGroup, personId, res => successfulDelete = res, err => deleteError = err);
        if(successfulDelete && OnPersonInGroupDeleted != null)
        {
            OnPersonInGroupDeleted.Invoke();
        }
    }

    public IEnumerator AddFaceToPersonInGroup(string personGroup, string personId, string[] imageFiles)
    {

        int facesAdded = 0;
        for (int i = 0; i < imageFiles.Length; i++)
        {
            bool added = false;
            string faceId = "";
            yield return RequestManager.AddFaceToPersonInGroup(m_Endpoint, m_ApiKey, personGroup, personId, imageFiles[i], "", res => added = res, value => faceId = value);
            if (added)
            {
                facesAdded++;
            }
        }
        if(facesAdded == imageFiles.Length && OnFacesAddedToPerson != null)
        {
            OnFacesAddedToPerson.Invoke();
        }

    }
}
