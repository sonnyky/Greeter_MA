using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseManager : MonoBehaviour
{
    GameObject m_MainCanvas;

    [SerializeField]
    GameObject m_AzureManager;

    [SerializeField]
    GameObject m_StandbyUi;

    [SerializeField]
    GameObject m_RegistrationUi;

    public enum AppState
    {
        REGISTRATION,
        STANDBY,
        INITIALIZE
    }

    AppState m_State = AppState.INITIALIZE;

    // Start is called before the first frame update
    void Start()
    {
        InstantiateUi(m_State);
        InstantiateManagers();
    }

    void InstantiateManagers()
    {
        GameObject azureMgr = Instantiate(m_AzureManager);
        azureMgr.name = "AzureManager";
        azureMgr.transform.SetParent(transform);
    }

    void InstantiateUi(AppState state)
    {
        m_MainCanvas = GameObject.FindGameObjectWithTag("Canvas");
        if (m_MainCanvas == null)
        {
            Debug.LogError(Constants.CANVAS_NOT_FOUND);
            return;
        }
        switch (state)
        {
            case AppState.STANDBY:
                GameObject main = Instantiate(m_StandbyUi);
                main.transform.SetParent(m_MainCanvas.transform, false);
                main.transform.localPosition = Vector3.zero;
                main.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            case AppState.REGISTRATION:
                GameObject reg = Instantiate(m_RegistrationUi);
                reg.transform.SetParent(m_MainCanvas.transform, false);
                reg.transform.localPosition = Vector3.zero;
                reg.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            default:
                break;
        }
    }

    public AppState GetState()
    {
        return m_State;
    }

}
