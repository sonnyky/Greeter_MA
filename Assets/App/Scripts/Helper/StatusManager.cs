using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    Text m_StatusText;

    public void ShowStatus(string status)
    {
        m_StatusText.text = status;
    }
}
