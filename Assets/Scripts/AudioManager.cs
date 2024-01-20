using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource m_oneShotAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void PlayOneShot(AudioClip a_audioClip, float a_volume = 1f)
    {
        m_oneShotAudioSource.PlayOneShot(a_audioClip, a_volume);
    }

}
