using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_StandbyUi;

    [SerializeField]
    GameObject m_RegistrationUi;

    GameObject m_MainCanvas;

    enum AppState
    {
        REGISTRATION,
        STANDBY
    }

    AppState m_State = AppState.STANDBY;

    // Start is called before the first frame update
    void Start()
    {
        m_MainCanvas = GameObject.FindGameObjectWithTag("Canvas");
        if(m_MainCanvas == null)
        {
            Debug.LogError(Constants.CANVAS_NOT_FOUND);
        }
        else
        {
            InstantiateUi(m_State);
        }
    }

    void InstantiateUi(AppState state)
    {
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

}
