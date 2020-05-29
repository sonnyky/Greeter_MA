using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    AudioClip[] m_ListOfClips;

    AudioSource m_AudioSource;

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(int clipNum)
    {
        if (clipNum >= m_ListOfClips.Length) return;
        m_AudioSource.Stop();
        m_AudioSource.clip = m_ListOfClips[clipNum];
        m_AudioSource.Play();
    }
}
