using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    BattleHandler m_battleHandlerRef;
    [SerializeField] GameObject m_resumeButtonRef;
    [SerializeField] GameObject m_perishButtonRef;
    [SerializeField] GameObject m_quitButtonRef;

    [SerializeField] Slider m_masterAudioSlider;
    [SerializeField] Slider m_SFXAudioSlider;
    [SerializeField] Slider m_ambienceAudioSlider;
    [SerializeField] Slider m_musicAudioSlider;
    [SerializeField] UICheckBox m_masterCheckBox;
    [SerializeField] UICheckBox m_SFXCheckBox;
    [SerializeField] UICheckBox m_ambienceCheckBox;
    [SerializeField] UICheckBox m_musicCheckBox;

    // Start is called before the first frame update
    void Awake()
    {
        m_perishButtonRef.SetActive(false);
        m_masterAudioSlider.value = GameHandler._audioManager.GetChannelVolume(AudioManager.eSoundChannel.Master);
        m_SFXAudioSlider.value = GameHandler._audioManager.GetChannelVolume(AudioManager.eSoundChannel.SFX);
        m_ambienceAudioSlider.value = GameHandler._audioManager.GetChannelVolume(AudioManager.eSoundChannel.Ambience);
        m_musicAudioSlider.value = GameHandler._audioManager.GetChannelVolume(AudioManager.eSoundChannel.Music);
        m_masterCheckBox.SetToggled(GameHandler._audioManager.GetChannelEnabled(AudioManager.eSoundChannel.Master));
        m_SFXCheckBox.SetToggled(GameHandler._audioManager.GetChannelEnabled(AudioManager.eSoundChannel.SFX));
        m_musicCheckBox.SetToggled(GameHandler._audioManager.GetChannelEnabled(AudioManager.eSoundChannel.Music));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    internal void Init(BattleHandler a_battleHandler)
    {
        m_battleHandlerRef = a_battleHandler;
        m_perishButtonRef.SetActive(true);
    }

    public void Perish()
    {
        m_battleHandlerRef.Perish();
        Resume();
    }

    public void Resume()
    {
        Destroy(gameObject);
        if (m_battleHandlerRef != null)
        {
            m_battleHandlerRef.SetPaused(false);
        }
    }

    public void QuitGame()
    {
        GameHandler.Quit();
    }

    public void ChangeMasterVolume()
    {
        GameHandler._audioManager.SetChannelVolume(AudioManager.eSoundChannel.Master, m_masterAudioSlider.value);
    }

    public void ChangeSFXVolume()
    {
        GameHandler._audioManager.SetChannelVolume(AudioManager.eSoundChannel.SFX, m_SFXAudioSlider.value);
    }

    public void ChangeAmbienceVolume()
    {
        GameHandler._audioManager.SetChannelVolume(AudioManager.eSoundChannel.Ambience, m_ambienceAudioSlider.value);
    }

    public void ChangeMusicVolume()
    {
        GameHandler._audioManager.SetChannelVolume(AudioManager.eSoundChannel.Music, m_musicAudioSlider.value);

    }
    public void ToggleMaster()
    {
        m_SFXCheckBox.Toggle();
        GameHandler._audioManager.SetChannelEnabled(AudioManager.eSoundChannel.SFX, m_SFXCheckBox.GetToggled());
    }

    public void ToggleSFX()
    {
        m_SFXCheckBox.Toggle();
        GameHandler._audioManager.SetChannelEnabled(AudioManager.eSoundChannel.SFX, m_SFXCheckBox.GetToggled());
    }

    public void ToggleAmbience()
    {
        m_ambienceCheckBox.Toggle();
        GameHandler._audioManager.SetChannelEnabled(AudioManager.eSoundChannel.Ambience, m_ambienceCheckBox.GetToggled());
    }

    public void ToggleMusic()
    {
        m_musicCheckBox.Toggle();
        GameHandler._audioManager.SetChannelEnabled(AudioManager.eSoundChannel.Music, m_musicCheckBox.GetToggled());
    }
}
