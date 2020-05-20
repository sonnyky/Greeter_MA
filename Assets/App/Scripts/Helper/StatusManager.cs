using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_StatusText;

    public void ShowStatus(string status)
    {
        m_StatusText.text = status;
    }
}
