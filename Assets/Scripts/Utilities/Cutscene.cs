using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    [SerializeField] GameObject m_panelRef;
    [SerializeField] TextMeshProUGUI m_descriptionTextRef;

    string m_descriptionString;
    int m_charactersShown = 0;
    vTimer m_printTimer;
    bool m_printing = false;

    //Images
    bool m_usingImages = false;
    [SerializeField] Image m_sceneImage;
    vTimer m_imageFadeTimer;
    bool m_imageFadingIn = true;
    bool m_imageFadingOut = false;

    bool m_writingText = false;

    float m_printScreen = 1f;
    const float m_characterPrintTime = 0.03f;

    List<string> m_dialogList;
    List<Sprite> m_sceneSprites;

    float m_flashStrength = 0f;
    const float m_flashDecayMultiplier = 0.99f;

    ZoomExpandComponent m_closingZoom;
    internal delegate void OnCloseDelegate();
    internal OnCloseDelegate m_onCloseDelegate;

    [SerializeField] TextMeshProUGUI m_pressToContinueTextRef;
    bool m_showingPressToContinueText = false;

    internal void AddDialog(string a_string) { m_dialogList.Add(a_string); AssignDescription(); } 

    internal void SetPrintSpeed(float a_printSpeed) { m_printScreen = a_printSpeed; RefreshPrintTimer(); }

    // Start is called before the first frame update
    void Start()
    {
        ZoomExpandComponent closingZoom = m_panelRef.AddComponent<ZoomExpandComponent>();
        closingZoom.Init();
        closingZoom.SetFinishDelegate(Open);
        m_imageFadeTimer = new vTimer(0.5f);
        m_imageFadeTimer.SetClampingTimer(true);
    }

    internal void Init(bool a_usingImages, OnCloseDelegate a_onCloseDelegate = null)
    {
        m_usingImages = a_usingImages;
        m_sceneImage.gameObject.SetActive(m_usingImages);
        m_onCloseDelegate = a_onCloseDelegate;
        m_dialogList = new List<string>();
        m_sceneSprites = new List<Sprite>();
        m_descriptionString = "";
        RefreshPrintTimer();
    }

    internal void AddDialogs(List<string> a_dialogs)
    {
        for (int i = 0; i < a_dialogs.Count; i++)
        {
            AddDialog(a_dialogs[i]);
        }
        AssignDescription();
    }

    internal void AddImages(List<Sprite> a_sprites)
    {
        for (int i = 0; i < a_sprites.Count; i++)
        {
            m_sceneSprites.Add(a_sprites[i]);
        }
        AssignSceneSprite();
    }

    void RefreshPrintTimer()
    {
        m_printTimer = new vTimer(m_characterPrintTime / m_printScreen, false, true, true, true);
    }

    void TextUpdate()
    {
        if (m_writingText && m_charactersShown < m_descriptionString.Length)
        {
            if (m_printTimer.Update())
            {
                m_charactersShown++;
                m_descriptionTextRef.maxVisibleCharacters = m_charactersShown;

                if (m_descriptionTextRef.text[m_descriptionTextRef.text.Length - 1] != ' ')
                {
                    m_flashStrength = 1f;
                }
            }
            m_printing = true;
        }
        else
        {
            m_printing = false;
        }

        m_showingPressToContinueText = !m_printing;
        m_flashStrength *= m_flashDecayMultiplier;
    }

    void ImageUpdate()
    {
        if (m_imageFadingOut)
        {
            if (m_imageFadeTimer.Update())
            {
                m_imageFadingOut = false;
                m_imageFadingIn = true;
                AssignSceneSprite();
            }
            float compPercentage = m_imageFadeTimer.GetCompletionPercentage();
            m_sceneImage.color = Color.white.WithAlpha(1f-compPercentage);
        }
        else if (m_imageFadingIn)
        {
            if (m_imageFadeTimer.Update())
            {
                m_imageFadingIn = false;
            }
            else
            {
                float compPercentage = m_imageFadeTimer.GetCompletionPercentage();
                m_sceneImage.color = Color.white.WithAlpha(compPercentage);
            }
        }
    }
    void PressToContinueTextUpdate()
    {
        if (m_showingPressToContinueText)
        {
            float sinT = Mathf.Abs(Mathf.Sin(Time.time * 2f)) * 0.5f + 0.5f;
            m_pressToContinueTextRef.color = new Color(sinT, sinT, sinT, sinT);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            DialogPressed();
        }
        TextUpdate();
        if (m_usingImages)
        {
            ImageUpdate();
        }
        PressToContinueTextUpdate();
    }

    void AssignDescription()
    {
        m_descriptionString = m_dialogList[0];
        m_descriptionTextRef.text = m_descriptionString;

        m_charactersShown = 0;
        m_descriptionTextRef.maxVisibleCharacters = m_charactersShown;

        RefreshPrintTimer();
    }

    void AssignSceneSprite()
    {
        m_sceneImage.sprite = m_sceneSprites[0];
    }

    void ProgressDialog()
    {
        if (m_sceneSprites.Count > 1)
        {
            m_sceneSprites.RemoveAt(0);
            if (m_sceneImage.sprite != m_sceneSprites[0])
            {
                m_imageFadingOut = true;
            }
        }

        if (m_dialogList.Count > 1)
        {
            m_dialogList.RemoveAt(0);
            AssignDescription();
        }
        else if (m_closingZoom == null)
        {
            m_closingZoom = m_panelRef.AddComponent<ZoomExpandComponent>();
            m_closingZoom.Init(1f,0f,0.3f,2f, Close);
        }
    }

    public void DialogPressed()
    {
        if (m_printing)
        {
            m_charactersShown = m_descriptionString.Length-1;
        }
        else
        {
            ProgressDialog();
        }
    }

    void Open()
    {
        m_writingText = true;
    }

    void Close()
    {
        if (m_onCloseDelegate != null)
        {
            m_onCloseDelegate.Invoke();
        }
        Destroy(gameObject);
    }
}