using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseManager : MonoBehaviour
{
    GameObject m_MainCanvas;

    [SerializeField]
    GameObject m_UiDisplay;

    [SerializeField]
    GameObject m_AzureManager;

    CaptureManager m_CaptureManager;

    [SerializeField]
    GameObject m_DoorManager;

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
        InstantiateUi();
        InstantiateManagers();
        m_CaptureManager = m_UiDisplay.GetComponent<CaptureManager>();
    }

    void InstantiateManagers()
    {
        GameObject azureMgr = Instantiate(m_AzureManager);
        azureMgr.name = "AzureManager";
        azureMgr.transform.SetParent(transform);

        GameObject doorManager = Instantiate(m_DoorManager);
        doorManager.name = "DoorManager";
        doorManager.transform.SetParent(transform);
    }

    void InstantiateUi()
    {
        m_MainCanvas = GameObject.FindGameObjectWithTag("Canvas");
        if (m_MainCanvas == null)
        {
            Debug.LogError(Constants.CANVAS_NOT_FOUND);
            return;
        }
      
        GameObject main = Instantiate(m_UiDisplay);
        main.transform.SetParent(m_MainCanvas.transform, false);
        main.transform.localPosition = Vector3.zero;
        main.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public AppState GetState()
    {
        return m_State;
    }

}
