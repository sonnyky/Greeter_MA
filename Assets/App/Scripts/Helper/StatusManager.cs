using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_StatusText;

    [SerializeField]
    TextMeshProUGUI m_DebugText;

    [SerializeField]
    Sprite[] m_DetectionStatusIconSprites;

    [SerializeField]
    Image m_DetectionStatusIcon;

    WaitForSeconds m_TimeUntilHideIcon = new WaitForSeconds(3f);

    public void ShowStatus(string status)
    {
        m_StatusText.text = status;
    }

    public void SetDetectionIcon(int status)
    {
        m_DetectionStatusIcon.gameObject.SetActive(true);
        m_DetectionStatusIcon.sprite = m_DetectionStatusIconSprites[status];
        StartCoroutine(HideDetectionIcon());
    }

    IEnumerator HideDetectionIcon()
    {
        yield return m_TimeUntilHideIcon;
        m_DetectionStatusIcon.gameObject.SetActive(false);
    }

    public void ShowDebugStatus(string status)
    {
        m_DebugText.text = status;
    }
}
