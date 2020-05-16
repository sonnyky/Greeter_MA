using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_MainUi;

    GameObject m_MainCanvas;

    // Start is called before the first frame update
    void Start()
    {
        m_MainCanvas = GameObject.FindGameObjectWithTag("Canvas");
        if(m_MainCanvas != null)
        {
            GameObject main = Instantiate(m_MainUi);
            main.transform.SetParent(m_MainCanvas.transform, false);
            main.transform.localPosition = Vector3.zero;
            main.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

}
