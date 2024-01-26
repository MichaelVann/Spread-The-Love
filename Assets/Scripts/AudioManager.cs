using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    internal float m_sceneFadeAmount = 0f;

    internal enum eSoundChannel
    {
        Master,
        SFX,
        Music,
        Ambience,
    }

    struct SoundChannel
    {
        internal eSoundChannel channelCode;
        internal bool enabled;
        internal float volume;
        internal string exposedParameterName;
    }

    string[] exposedParameters = { "MasterVol",  "SFXVol", "MusicVol", "AmbienceVol" };

    SoundChannel[] m_soundChannels;

    [SerializeField] AudioMixer m_audioMixer;

    [SerializeField] AudioSource m_SFXAudioSource;

    internal void ToggleSoundChannel(eSoundChannel a_soundChannel) { m_soundChannels[(int)a_soundChannel].enabled = !m_soundChannels[(int)a_soundChannel].enabled; Refresh(); }

    internal void SetChannelVolume(eSoundChannel a_channel, float a_volume) { m_soundChannels[(int)a_channel].volume = a_volume; Refresh(); }
    internal float GetChannelVolume(eSoundChannel a_channel) { return m_soundChannels[(int)a_channel].volume; }
    internal void SetChannelEnabled(eSoundChannel a_channel, bool a_enabled) { m_soundChannels[(int)a_channel].enabled = a_enabled; Refresh(); }
    internal bool GetChannelEnabled(eSoundChannel a_channel) { return m_soundChannels[(int)a_channel].enabled; }

    // Start is called before the first frame update
    void Start()
    {
        m_soundChannels = new SoundChannel[4];
        for (int i = 0; i < m_soundChannels.Length; i++)
        {
            m_soundChannels[i].channelCode = (eSoundChannel)i;
            m_soundChannels[i].volume = 1f;
            m_soundChannels[i].enabled = true;
            m_soundChannels[i].exposedParameterName = exposedParameters[i];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetupAudioSource(AudioSource a_audioSource, AudioClip a_clip, string a_mixerGroupName)
    {
        a_audioSource.clip = a_clip;
        a_audioSource.loop = true;
        a_audioSource.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups(a_mixerGroupName)[0];
    }

    float VolumeScaleToDB(float a_volume)
    {
        float volInDB = -80f;

        if (a_volume > 0f)
        {
            const float mult = 33.2f; // 10/Log(2)
            volInDB = mult * Mathf.Log10(a_volume);
        }

        return volInDB;
    }

    void UpdateSoundChannel(SoundChannel a_channel)
    {
        float vol = 1f;
        vol *= a_channel.volume;
        vol *= a_channel.enabled ? 1f : 0f;
        vol *= 1f-m_sceneFadeAmount;
        m_audioMixer.SetFloat(a_channel.exposedParameterName, VolumeScaleToDB(vol));
    }

    void UpdateVolumes()
    {
        for (int i = 0; i < m_soundChannels.Length; i++)
        {
            UpdateSoundChannel(m_soundChannels[i]);
        }
    }

    internal void Refresh()
    {
        UpdateVolumes();
    }

    internal void PlaySFX(AudioClip a_audioClip, float a_volume = 1f)
    {
        if (m_soundChannels[(int)eSoundChannel.SFX].enabled)
        {
            m_SFXAudioSource.PlayOneShot(a_audioClip, a_volume);
        }
    }

}
