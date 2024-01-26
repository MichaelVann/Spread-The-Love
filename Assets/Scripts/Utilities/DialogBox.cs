using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    [SerializeField] GameObject m_parentObject;
    [SerializeField] TextMeshProUGUI m_descriptionTextRef;
    [SerializeField] Image m_sceneImage;
    vTimer m_imageFadeTimer;
    bool m_fadingOut = false;
    bool m_fadingIn = true;

    string m_descriptionString;
    int m_charactersShown = 0;
    vTimer m_printTimer;
    bool m_printing = false;

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

    internal void AddDialog(string a_string) { m_dialogList.Add(a_string); m_descriptionString = m_dialogList[0]; } 

    internal void SetPrintSpeed(float a_printSpeed) { m_printScreen = a_printSpeed; RefreshPrintTimer(); }

    // Start is called before the first frame update
    void Start()
    {
        ZoomExpandComponent closingZoom = gameObject.AddComponent<ZoomExpandComponent>();
        closingZoom.Init();
        closingZoom.SetFinishDelegate(Open);
        m_imageFadeTimer = new vTimer(0.5f);
        m_imageFadeTimer.SetClampingTimer(true);
    }

    internal void Init(OnCloseDelegate a_onCloseDelegate = null)
    {
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
        m_flashStrength *= m_flashDecayMultiplier;
    }

    void ImageUpdate()
    {
        if (m_fadingOut)
        {
            if (m_imageFadeTimer.Update())
            {
                m_fadingOut = false;
                m_fadingIn = true;
                AssignSceneSprite();
            }
            float compPercentage = m_imageFadeTimer.GetCompletionPercentage();
            m_sceneImage.color = Color.white.WithAlpha(1f-compPercentage);
        }
        else if (m_fadingIn)
        {
            if (m_imageFadeTimer.Update())
            {
                m_fadingIn = false;
            }
            else
            {
                float compPercentage = m_imageFadeTimer.GetCompletionPercentage();
                m_sceneImage.color = Color.white.WithAlpha(compPercentage);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        TextUpdate();
        ImageUpdate();
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
                m_fadingOut = true;
            }
        }

        if (m_dialogList.Count > 1)
        {
            m_dialogList.RemoveAt(0);
            AssignDescription();
        }
        else if (m_closingZoom == null)
        {
            m_closingZoom = gameObject.AddComponent<ZoomExpandComponent>();
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
        Destroy(m_parentObject);
    }
}